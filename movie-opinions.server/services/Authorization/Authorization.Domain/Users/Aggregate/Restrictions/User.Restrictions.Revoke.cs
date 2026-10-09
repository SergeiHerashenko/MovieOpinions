using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using Authorization.Domain.Users.DomainEvents.SessionRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.Enums;
using Authorization.Domain.Users.Errors;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        #region RemoveRestrictions
        /// <summary>
        /// Знімає одне або кілька активних обмежень користувача.
        ///
        /// Перевіряє передані ідентифікатори, групує знайдені обмеження
        /// за типом і готує план для кожної відповідної сесії без зміни
        /// стану агрегату.
        ///
        /// Після успішної підготовки всіх планів атомарно застосовує
        /// переходи обмежень і сесій, а потім записує доменні події
        /// та зміни агрегату.
        ///
        /// Залежно від часового стану сесії обмеження можуть бути
        /// відкликані або завершені.
        ///
        /// (Removes one or more active user restrictions.
        ///
        /// Validates the supplied identifiers, groups matching restrictions
        /// by type, and prepares a plan for every corresponding session
        /// without modifying the aggregate.
        ///
        /// After every plan has been successfully prepared, atomically
        /// applies restriction and session transitions and then records
        /// domain events and aggregate changes.
        ///
        /// Depending on the session’s temporal state, restrictions may
        /// be revoked or completed.)
        /// </summary>
        /// <param name="restrictionIds">Ідентифікатори обмежень, які необхідно зняти.</param>
        /// <param name="now">Фіксований час виконання всієї операції.</param>
        /// <returns>
        /// Успіх або очікувана помилка, якщо список порожній, містить
        /// дублікати, деякі обмеження не знайдені чи не можуть бути відкликані.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо колекція відсутня, містить null-ідентифікатори
        /// або переданий час є некоректним.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо стан обмежень і відповідних сесій неузгоджений.
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо отримано непідтримуваний результат перевірки.
        /// </exception>
        public Result RemoveRestrictions(
            IEnumerable<UserRestrictionId> restrictionIds,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (restrictionIds, nameof(restrictionIds))
            );

            DomainGuard.AgainstEarlierThan<User>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            var requestedRestrictionIds = restrictionIds.ToList();

            if (requestedRestrictionIds.Count == 0)
                return Result.Failure(UserErrors.EmptyRestrictionList<User>());

            if (requestedRestrictionIds.Any(x => x is null))
            {
                throw DomainInvalidOperationException.PreconditionFailed<User>(
                    nameof(RemoveRestrictions),
                    "All restriction identifiers must be non-null.",
                    OperationType.Update,
                    context: new Dictionary<string, object>
                    {
                        ["RestrictionIdentifierCount"] = requestedRestrictionIds.Count,
                        ["NullIdentifierCount"] = requestedRestrictionIds.Count(x => x is null)
                    }
                );
            }

            var uniqueRequestedRestrictionIds = requestedRestrictionIds.ToHashSet();

            if (uniqueRequestedRestrictionIds.Count != requestedRestrictionIds.Count)
                return Result.Failure(UserErrors.DuplicateRestrictionIdentifiers<User>());

            var matchedRestrictions = _restrictions
                .Where(x => uniqueRequestedRestrictionIds.Contains(x.Id))
                .ToArray();

            var matchedRestrictionIds = matchedRestrictions
                .Select(x => x.Id)
                .ToHashSet();

            var missingRestrictionIds = uniqueRequestedRestrictionIds
                .Except(matchedRestrictionIds)
                .ToArray();

            if (missingRestrictionIds.Length > 0)
                return Result.Failure(UserErrors.NotFoundRestriction<User>(missingRestrictionIds));

            var removalPlansResult = PrepareRestrictionRemovalPlans(
                matchedRestrictions,
                now
            );

            if (removalPlansResult.IsFailure)
                return Result.Failure(removalPlansResult.Errors);

            ApplyRestrictionRemovalPlans(
                removalPlansResult.Value,
                now
            );

            RecordRestrictionRemovalEventsAndChanges(
                removalPlansResult.Value,
                now
            );

            return Result.Success();
        }
        #endregion

        #region Removal Plans
        /// <summary>
        /// Представляє попередньо перевірений виконуваний план
        /// зміни обмежень у межах однієї сесії.
        ///
        /// План окремо визначає мутацію доменного стану та запис
        /// подій і змін агрегату. Його створення не змінює агрегат.
        ///
        /// (Represents a pre-validated executable plan for changing
        /// restrictions within a single session.
        ///
        /// The plan separately defines domain-state mutation and the
        /// recording of events and aggregate changes. Creating the plan
        /// does not modify the aggregate.)
        /// </summary>
        private abstract class RestrictionRemovalPlan
        {
            public UserRestrictionSession Session { get; }

            protected RestrictionRemovalPlan(UserRestrictionSession session)
            {
                Session = session;
            }

            /// <summary>
            /// Застосовує попередньо перевірені переходи стану,
            /// визначені цим планом.
            ///
            /// Метод повинен викликатися лише після успішної підготовки
            /// всіх планів операції.
            ///
            /// (Applies the pre-validated state transitions represented
            /// by this plan.
            ///
            /// The method must be called only after every operation plan
            /// has been successfully prepared.)
            /// </summary>
            /// <param name="user">Агрегат, до якого застосовується план.</param>
            /// <param name="now">Фіксований час виконання операції.</param>
            public abstract void Apply(
                User user,
                DateTimeOffset now);

            /// <summary>
            /// Записує доменні події та зміни агрегату, що відповідають
            /// уже застосованому плану.
            ///
            /// Метод не виконує повторних переходів доменного стану.
            ///
            /// (Records domain events and aggregate changes corresponding
            /// to a plan that has already been applied.
            ///
            /// The method does not perform domain-state transitions again.)
            /// </summary>
            /// <param name="user">Агрегат, у якому записуються наслідки.</param>
            /// <param name="now">Фіксований час виконання операції.</param>
            public abstract void RecordEventsAndChanges(
                User user,
                DateTimeOffset now);
        }

        /// <summary>
        /// Представляє план відкликання частини обмежень,
        /// після якого сесія продовжує існувати.
        ///
        /// Вибрані обмеження відкликаються, а сесія оновлюється
        /// відповідно до обмежень, що залишилися.
        ///
        /// (Represents a plan for revoking part of a session’s restrictions
        /// while the session continues to exist.
        ///
        /// Selected restrictions are revoked, and the session is updated
        /// according to the restrictions that remain.)
        /// </summary>
        private sealed class SessionRemainsActivePlan : RestrictionRemovalPlan
        {
            public IReadOnlyCollection<UserRestriction> RestrictionsToRevoke { get; }

            public SessionRemainsActivePlan(
                UserRestrictionSession session,
                IReadOnlyCollection<UserRestriction> restrictionsToRevoke)
                : base(session)
            {
                RestrictionsToRevoke = restrictionsToRevoke.ToArray();
            }

            public override void Apply(
                User user,
                DateTimeOffset now)
            {
                foreach (var restriction in RestrictionsToRevoke)
                {
                    user.RevokeAndRemoveRestriction(
                        restriction,
                        now
                    );
                }

                Session.RemoveRestrictions(
                    RestrictionsToRevoke,
                    now
                );
            }

            public override void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddDomainEvent(new UserRestrictionsRemovedFromSessionEvent(
                    Session.Id,
                    Session.RestrictionType,
                    RestrictionsToRevoke,
                    user.Login,
                    Session.TotalBlockedMinutes,
                    now)
                );

                user.AddRestrictionUpdatedChanges(
                    RestrictionsToRevoke,
                    now
                );

                user.AddAggregateChange(new UserRestrictionSessionUpdated(
                    Session,
                    now)
                );
            }
        }

        /// <summary>
        /// Представляє план завершення сесії, яка вже була
        /// прострочена на момент виконання операції.
        ///
        /// Усі активні обмеження сесії переходять у завершений стан,
        /// після чого сесія видаляється.
        ///
        /// (Represents a plan for completing a session that was already
        /// expired when the operation started.
        ///
        /// All active restrictions in the session transition to the
        /// completed state, after which the session is removed.)
        /// </summary>
        private sealed class SessionAlreadyExpiredPlan : RestrictionRemovalPlan
        {
            public IReadOnlyCollection<UserRestriction> RestrictionsToComplete { get; }

            public SessionAlreadyExpiredPlan(
                UserRestrictionSession session,
                IReadOnlyCollection<UserRestriction> restrictionsToComplete)
                : base(session)
            {
                RestrictionsToComplete = restrictionsToComplete.ToArray();
            }

            public override void Apply(
                User user,
                DateTimeOffset now)
            {
                foreach (var restriction in RestrictionsToComplete)
                {
                    user.CompleteAndRemoveRestriction(
                        restriction,
                        now
                    );
                }

                Session.RemoveRestrictions(
                    RestrictionsToComplete,
                    now
                );

                user.RemoveRestrictionSession(Session);
            }

            public override void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddDomainEvent(new UserRestrictionSessionCompletedEvent(
                    Session.Id,
                    user.Login,
                    Session.RestrictionType,
                    now)
                );

                user.AddRestrictionUpdatedChanges(
                    RestrictionsToComplete,
                    now
                );

                user.AddAggregateChange(new UserRestrictionSessionDeleted(
                    Session.Id,
                    now)
                );
            }
        }

        /// <summary>
        /// Представляє план для сесії, яка стає простроченою
        /// внаслідок відкликання вибраних обмежень.
        ///
        /// Вибрані обмеження відкликаються, решта переходять
        /// у завершений стан, після чого сесія видаляється.
        ///
        /// (Represents a plan for a session that becomes expired
        /// as a result of revoking selected restrictions.
        ///
        /// Selected restrictions are revoked, the remaining restrictions
        /// transition to the completed state, and the session is removed.)
        /// </summary>
        private sealed class SessionBecomesExpiredPlan : RestrictionRemovalPlan
        {
            public IReadOnlyCollection<UserRestriction> RestrictionsToRevoke { get; }

            public IReadOnlyCollection<UserRestriction> RestrictionsToComplete { get; }

            public SessionBecomesExpiredPlan(
                UserRestrictionSession session,
                IReadOnlyCollection<UserRestriction> restrictionsToRevoke,
                IReadOnlyCollection<UserRestriction> restrictionsToComplete)
                : base(session)
            {
                RestrictionsToRevoke = restrictionsToRevoke.ToArray();
                RestrictionsToComplete = restrictionsToComplete.ToArray();
            }

            public override void Apply(
                User user,
                DateTimeOffset now)
            {
                foreach (var restriction in RestrictionsToRevoke)
                {
                    user.RevokeAndRemoveRestriction(
                        restriction,
                        now
                    );
                }

                foreach (var restriction in RestrictionsToComplete)
                {
                    user.CompleteAndRemoveRestriction(
                        restriction,
                        now
                    );
                }

                var restrictionsToRemoveFromSession = RestrictionsToComplete
                    .Concat(RestrictionsToRevoke)
                    .ToArray();

                Session.RemoveRestrictions(
                    restrictionsToRemoveFromSession,
                    now
                );

                user.RemoveRestrictionSession(Session);
            }

            public override void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddDomainEvent(new UserRestrictionSessionCompletedEvent(
                    Session.Id,
                    user.Login,
                    Session.RestrictionType,
                    now)
                );

                var restrictions = RestrictionsToComplete
                    .Concat(RestrictionsToRevoke)
                    .ToArray();

                user.AddRestrictionUpdatedChanges(
                    restrictions,
                    now
                );

                user.AddAggregateChange(new UserRestrictionSessionDeleted(
                    Session.Id,
                    now)
                );
            }
        }

        /// <summary>
        /// Представляє план відкликання всіх обмежень сесії.
        ///
        /// Після відкликання обмежень сесія стає порожньою
        /// та повністю скасовується.
        ///
        /// (Represents a plan for revoking every restriction in a session.
        ///
        /// After the restrictions are revoked, the session becomes empty
        /// and is completely revoked.)
        /// </summary>
        private sealed class SessionBecomesEmptyPlan : RestrictionRemovalPlan
        {
            public IReadOnlyCollection<UserRestriction> RestrictionsToRevoke { get; }

            public SessionBecomesEmptyPlan(
                UserRestrictionSession session,
                IReadOnlyCollection<UserRestriction> restrictionsToRevoke)
                : base(session)
            {
                RestrictionsToRevoke = restrictionsToRevoke.ToArray();
            }

            public override void Apply(
                User user,
                DateTimeOffset now)
            {
                foreach (var restriction in RestrictionsToRevoke)
                {
                    user.RevokeAndRemoveRestriction(
                        restriction,
                        now
                    );
                }

                Session.RemoveRestrictions(
                    RestrictionsToRevoke,
                    now
                );

                user.RemoveRestrictionSession(Session);
            }

            public override void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddDomainEvent(new UserRestrictionSessionRevokedEvent(
                    Session.Id,
                    user.Login,
                    Session.RestrictionType,
                    now)
                );

                user.AddRestrictionUpdatedChanges(
                    RestrictionsToRevoke,
                    now
                );

                user.AddAggregateChange(new UserRestrictionSessionDeleted(
                    Session.Id,
                    now)
                );
            }
        }
        #endregion

        #region Removal Plan Preparation
        /// <summary>
        /// Групує запитані обмеження за типом і готує
        /// виконуваний план для кожної відповідної сесії.
        ///
        /// Перевіряє існування сесії, точну відповідність її
        /// ідентифікаторів активним обмеженням агрегату та визначає
        /// наслідок видалення вибраних обмежень.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Groups requested restrictions by type and prepares
        /// an executable plan for every corresponding session.
        ///
        /// Verifies that the session exists, exactly matches the
        /// aggregate’s active restrictions, and determines the effect
        /// of removing the selected restrictions.
        ///
        /// The method does not modify the aggregate state.)
        /// </summary>
        /// <param name="requestedRestrictions">Знайдені в агрегаті обмеження, які було запитано зняти.</param>
        /// <param name="now">Фіксований час виконання операції.</param>
        /// <returns>
        /// Підготовлені плани або помилка, якщо хоча б одне
        /// обмеження не може бути відкликане.
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо активні обмеження не мають відповідної сесії
        /// або склад сесії не збігається зі станом агрегату.
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо сесія повернула непідтримуваний результат перевірки.
        /// </exception>
        private Result<IReadOnlyCollection<RestrictionRemovalPlan>> PrepareRestrictionRemovalPlans(
            IReadOnlyCollection<UserRestriction> requestedRestrictions,
            DateTimeOffset now)
        {
            var plans = new List<RestrictionRemovalPlan>();

            foreach (var restrictionGroup in requestedRestrictions.GroupBy(x => x.RestrictionType))
            {
                var restrictionType = restrictionGroup.Key;
                var requestedRestrictionsOfType = restrictionGroup.ToList();

                var restrictionSession = _restrictionSessions
                    .FirstOrDefault(x => x.RestrictionType == restrictionType);

                var activeRestrictionsOfType = _restrictions
                    .Where(x => x.RestrictionType == restrictionType)
                    .ToArray();

                if (restrictionSession is null)
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "Active restrictions exist without a corresponding restriction session.",
                        new Dictionary<string, object?>
                        {
                            ["RestrictionType"] = restrictionType
                        },
                        OperationType.Update
                    );
                }

                if (!restrictionSession.ContainsExactly(activeRestrictionsOfType.Select(x => x.Id)))
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "The restriction session does not exactly match " +
                        "the active restrictions of its type.",
                        new Dictionary<string, object?>
                        {
                            ["UserId"] = Id.Value,
                            ["SessionId"] = restrictionSession.Id.Value,
                            ["RestrictionType"] = restrictionType,
                            ["SessionRestrictionIds"] = restrictionSession.ActiveRestrictionIds
                                .Select(x => x.Value)
                                .ToArray(),
                            ["AggregateRestrictionIds"] = activeRestrictionsOfType
                                .Select(x => x.Id.Value)
                                .ToArray()
                        },
                        OperationType.Update
                    );
                }

                var removalEffect = restrictionSession.ValidateCanRemoveRestrictions(
                    requestedRestrictionsOfType,
                    now
                );

                Result<RestrictionRemovalPlan> planResult = removalEffect switch
                {
                    RestrictionSessionRemovalEffect.AlreadyExpired =>
                        PrepareSessionAlreadyExpiredPlan(
                            restrictionSession,
                            now
                        ),

                    RestrictionSessionRemovalEffect.BecomesExpired =>
                        PrepareSessionBecomesExpiredPlan(
                            restrictionSession,
                            requestedRestrictionsOfType,
                            now
                        ),

                    RestrictionSessionRemovalEffect.BecomesEmpty =>
                        PrepareSessionBecomesEmptyPlan(
                            restrictionSession,
                            requestedRestrictionsOfType,
                            now
                        ),

                    RestrictionSessionRemovalEffect.RemainsActive =>
                        PrepareSessionRemainsActivePlan(
                            restrictionSession,
                            requestedRestrictionsOfType,
                            now
                        ),

                    _ => throw DomainDataInconsistencyException.UnsupportedDiscriminator<User>(
                        nameof(removalEffect),
                        removalEffect,
                        OperationType.Update
                    )
                };

                if (planResult.IsFailure)
                    return Result<IReadOnlyCollection<RestrictionRemovalPlan>>.Failure(planResult.Errors);

                plans.Add(planResult.Value);
            }

            return Result<IReadOnlyCollection<RestrictionRemovalPlan>>.Success(plans.AsReadOnly());
        }

        /// <summary>
        /// Готує план завершення сесії, яка вже прострочена.
        ///
        /// Знаходить усі активні обмеження її типу, перевіряє
        /// можливість завершення кожного з них і підтверджує
        /// очікуваний результат повного видалення із сесії.
        ///
        /// (Prepares a completion plan for an already expired session.
        ///
        /// Finds all active restrictions of its type, validates that each
        /// can be completed, and confirms the expected effect of removing
        /// all restrictions from the session.)
        /// </summary>
        /// <param name="restrictionSession">Прострочена сесія.</param>
        /// <param name="now">Фіксований час виконання операції.</param>
        /// <returns>Підготовлений план завершення сесії.</returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження не можуть бути завершені або повторна
        /// перевірка сесії повернула неочікуваний результат.
        /// </exception>
        private Result<RestrictionRemovalPlan> PrepareSessionAlreadyExpiredPlan(
            UserRestrictionSession restrictionSession,
            DateTimeOffset now)
        {
            var restrictionsToComplete = _restrictions
                .Where(x => x.RestrictionType == restrictionSession.RestrictionType)
                .ToArray();

            foreach (var restriction in restrictionsToComplete)
            {
                restriction.ValidateCanComplete(now);
            }

            var fullRemovalEffect = restrictionSession.ValidateCanRemoveRestrictions(
                restrictionsToComplete,
                now
            );

            if (fullRemovalEffect != RestrictionSessionRemovalEffect.AlreadyExpired)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "Full removal validation produced an unexpected effect " +
                    "for an already expired restriction session.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = Id.Value,
                        ["SessionId"] = restrictionSession.Id.Value,
                        ["RestrictionType"] =
                            restrictionSession.RestrictionType,
                        ["ExpectedRemovalEffect"] =
                            RestrictionSessionRemovalEffect.AlreadyExpired,
                        ["ActualRemovalEffect"] = fullRemovalEffect,
                        ["RestrictionIds"] = restrictionsToComplete
                            .Select(x => x.Id.Value)
                            .ToArray()
                    },
                    OperationType.Update
                );
            }

            var plan = new SessionAlreadyExpiredPlan(
                restrictionSession,
                restrictionsToComplete
            );

            return Result<RestrictionRemovalPlan>.Success(plan);
        }

        /// <summary>
        /// Готує план для сесії, яка стане простроченою
        /// після відкликання вибраних обмежень.
        ///
        /// Перевіряє можливість відкликання вибраних обмежень,
        /// завершення решти обмежень і повного очищення сесії.
        ///
        /// (Prepares a plan for a session that will become expired
        /// after selected restrictions are revoked.
        ///
        /// Validates revocation of the selected restrictions, completion
        /// of the remaining restrictions, and complete removal from the session.)
        /// </summary>
        /// <param name="restrictionSession">Сесія, яка стане простроченою.</param>
        /// <param name="restrictionsToRevoke">Обмеження, які необхідно відкликати.</param>
        /// <param name="now">Фіксований час виконання операції.</param>
        /// <returns>
        /// Підготовлений план або помилка, якщо хоча б одне вибране
        /// обмеження не може бути відкликане.
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо решта обмежень не можуть бути завершені
        /// або повне видалення не залишає сесію порожньою.
        /// </exception>
        private Result<RestrictionRemovalPlan> PrepareSessionBecomesExpiredPlan(
            UserRestrictionSession restrictionSession,
            IReadOnlyCollection<UserRestriction> restrictionsToRevoke,
            DateTimeOffset now)
        {
            var restrictionsToComplete = _restrictions
                .Except(restrictionsToRevoke)
                .Where(x => x.RestrictionType == restrictionSession.RestrictionType)
                .ToArray();

            foreach (var restriction in restrictionsToRevoke)
            {
                var revocationValidationResult = restriction.ValidateRevocation(now);

                if (revocationValidationResult.IsFailure)
                    return Result<RestrictionRemovalPlan>.Failure(revocationValidationResult.Errors);
            }

            foreach (var restriction in restrictionsToComplete)
            {
                restriction.ValidateCanComplete(now);
            }

            var restrictionsToRemoveFromSession = restrictionsToRevoke
                .Concat(restrictionsToComplete)
                .ToArray();

            var fullRemovalEffect = restrictionSession.ValidateCanRemoveRestrictions(
                restrictionsToRemoveFromSession,
                now
            );

            if (fullRemovalEffect != RestrictionSessionRemovalEffect.BecomesEmpty)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "Full removal validation did not produce an empty " +
                    "restriction session.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = Id.Value,
                        ["SessionId"] = restrictionSession.Id.Value,
                        ["RestrictionType"] =
                            restrictionSession.RestrictionType,
                        ["ExpectedRemovalEffect"] =
                            RestrictionSessionRemovalEffect.BecomesEmpty,
                        ["ActualRemovalEffect"] = fullRemovalEffect,
                        ["RestrictionIdsToRevoke"] = restrictionsToRevoke
                            .Select(x => x.Id.Value)
                            .ToArray(),
                        ["RestrictionIdsToComplete"] = restrictionsToComplete
                            .Select(x => x.Id.Value)
                            .ToArray()
                    },
                    OperationType.Update
                );
            }

            var plan = new SessionBecomesExpiredPlan(
                restrictionSession,
                restrictionsToRevoke,
                restrictionsToComplete
            );

            return Result<RestrictionRemovalPlan>.Success(plan);
        }

        /// <summary>
        /// Готує план відкликання всіх обмежень сесії,
        /// після якого вона стане порожньою.
        ///
        /// Метод перевіряє можливість відкликання кожного обмеження
        /// без зміни поточного стану агрегату.
        ///
        /// (Prepares a plan for revoking every restriction in a session,
        /// leaving the session empty.
        ///
        /// Validates that each restriction can be revoked without modifying
        /// the current aggregate state.)
        /// </summary>
        /// <param name="restrictionSession">Сесія, яка стане порожньою.</param>
        /// <param name="restrictionsToRevoke">Обмеження, які необхідно відкликати.</param>
        /// <param name="now">Фіксований час виконання операції.</param>
        /// <returns>
        /// Підготовлений план або помилка, якщо хоча б одне
        /// обмеження не може бути відкликане.
        /// </returns>
        private Result<RestrictionRemovalPlan> PrepareSessionBecomesEmptyPlan(
            UserRestrictionSession restrictionSession,
            IReadOnlyCollection<UserRestriction> restrictionsToRevoke,
            DateTimeOffset now)
        {
            foreach (var restriction in restrictionsToRevoke)
            {
                var revocationValidationResult = restriction.ValidateRevocation(now);

                if (revocationValidationResult.IsFailure)
                    return Result<RestrictionRemovalPlan>.Failure(revocationValidationResult.Errors);
            }

            var plan = new SessionBecomesEmptyPlan(
                restrictionSession,
                restrictionsToRevoke
            );

            return Result<RestrictionRemovalPlan>.Success(plan);
        }

        /// <summary>
        /// Готує план відкликання частини обмежень сесії,
        /// після якого вона продовжуватиме існувати.
        ///
        /// Метод перевіряє можливість відкликання кожного обмеження
        /// без зміни поточного стану агрегату.
        ///
        /// (Prepares a plan for revoking part of a session’s restrictions
        /// while the session continues to exist.
        ///
        /// Validates that each restriction can be revoked without modifying
        /// the current aggregate state.)
        /// </summary>
        /// <param name="restrictionSession">Сесія, яка продовжуватиме існувати.</param>
        /// <param name="restrictionsToRevoke">Обмеження, які необхідно відкликати.</param>
        /// <param name="now">Фіксований час виконання операції.</param>
        /// <returns>
        /// Підготовлений план або помилка, якщо хоча б одне
        /// обмеження не може бути відкликане.
        /// </returns>
        private Result<RestrictionRemovalPlan> PrepareSessionRemainsActivePlan(
            UserRestrictionSession restrictionSession,
            IReadOnlyCollection<UserRestriction> restrictionsToRevoke,
            DateTimeOffset now)
        {
            foreach (var restriction in restrictionsToRevoke)
            {
                var revocationValidationResult = restriction.ValidateRevocation(now);

                if (revocationValidationResult.IsFailure)
                    return Result<RestrictionRemovalPlan>.Failure(revocationValidationResult.Errors);
            }

            var plan = new SessionRemainsActivePlan(
                restrictionSession,
                restrictionsToRevoke
            );

            return Result<RestrictionRemovalPlan>.Success(plan); 
        }
        #endregion

        #region Removal Plan Application
        /// <summary>
        /// Послідовно застосовує всі попередньо перевірені
        /// плани зняття обмежень.
        ///
        /// Делегує кожному плану виконання відповідних переходів
        /// обмежень і сесії.
        ///
        /// Метод повинен викликатися лише після успішної
        /// підготовки всіх планів.
        ///
        /// (Sequentially applies all previously validated
        /// restriction-removal plans.
        ///
        /// Delegates the corresponding restriction and session
        /// transitions to each plan.
        ///
        /// The method must be called only after every plan has
        /// been successfully prepared.)
        /// </summary>
        /// <param name="removalPlans">Попередньо перевірені плани зняття обмежень.</param>
        /// <param name="now">Фіксований час виконання операції.</param>
        private void ApplyRestrictionRemovalPlans(
            IReadOnlyCollection<RestrictionRemovalPlan> removalPlans,
            DateTimeOffset now)
        {
            foreach (var plan in removalPlans)
            {
                plan.Apply(
                    this,
                    now
                );
            }
        }
        #endregion

        #region Removal Events And Changes
        /// <summary>
        /// Записує доменні події та зміни агрегату для всіх
        /// уже застосованих планів.
        ///
        /// Делегує кожному плану запис наслідків відповідного
        /// доменного сценарію. Метод не виконує повторних переходів стану.
        ///
        /// (Records domain events and aggregate changes for all plans
        /// that have already been applied.
        ///
        /// Delegates recording of each domain scenario’s consequences
        /// to the corresponding plan. The method does not perform
        /// state transitions again.)
        /// </summary>
        /// <param name="removalPlans">Застосовані плани зняття обмежень.</param>
        /// <param name="now">Фіксований час виконання операції.</param>
        private void RecordRestrictionRemovalEventsAndChanges(
            IReadOnlyCollection<RestrictionRemovalPlan> removalPlans,
            DateTimeOffset now)
        {
            foreach (var plan in removalPlans)
            {
                plan.RecordEventsAndChanges(
                    this,
                    now
                );
            }
        }
        #endregion
    }
}
