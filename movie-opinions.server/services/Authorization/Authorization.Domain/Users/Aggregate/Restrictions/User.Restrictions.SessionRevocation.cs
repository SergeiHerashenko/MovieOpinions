using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using Authorization.Domain.Users.DomainEvents.SessionRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.Enums;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.Errors;

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        #region Revoke Restriction Session
        /// <summary>
        /// Повністю припиняє сесію обмежень указаного типу.
        ///
        /// Перевіряє тип сесії, час операції та узгодженість
        /// наявності сесії з відповідними обмеженнями агрегату.
        ///
        /// Для активної сесії відкликає всі її обмеження.
        /// Для сесії із завершеним строком переводить
        /// усі її обмеження у завершений стан.
        ///
        /// Перед зміною стану перевіряє точну відповідність
        /// складу сесії її обмеженням і готує план операції.
        /// Після успішної підготовки застосовує план, вилучає сесію
        /// та записує доменну подію і зміни агрегату.
        ///
        /// (Completely terminates a restriction session of the specified type.
        ///
        /// Validates the session type, operation time, and consistency
        /// between session existence and the corresponding aggregate restrictions.
        ///
        /// Revokes all restrictions of an active session.
        /// For a session whose lifetime has ended,
        /// marks all its restrictions as completed.
        ///
        /// Before modifying state, verifies that the session's contents
        /// exactly match its restrictions and prepares the operation plan.
        /// After successful preparation, applies the plan, removes the session,
        /// and records a domain event and aggregate changes.)
        /// </summary>
        /// <param name="restrictionType">
        /// Тип сесії обмежень, яку необхідно повністю припинити.
        ///
        /// (Type of the restriction session to terminate completely.)
        /// </param>
        /// <param name="now">
        /// Час припинення сесії обмежень.
        ///
        /// (End time of the restrictions session.)
        /// </param>
        public Result RevokeRestrictionSession(
            RestrictionType restrictionType,
            DateTimeOffset now)
        {
            DomainGuard.AgainstUndefinedEnum<User>(
                OperationType.Delete,
                (restrictionType, nameof(restrictionType))
            );

            EnsureValidOperationTimestamp(now);

            var session = _restrictionSessions
                .FirstOrDefault(x => x.RestrictionType == restrictionType);

            var restrictions = _restrictions
                .Where(x => x.RestrictionType == restrictionType)
                .ToList();

            return (session, restrictions) switch
            {
                (null, []) => Result.Failure(UserErrors.NotFoundRestrictionSession<User>(restrictionType)),

                (not null, []) => throw DomainInvariantViolationException.BrokenState<User>(
                    $"Inconsistent aggregate state: session for '{restrictionType}' " +
                    "exists, but restrictions are missing.",
                    new Dictionary<string, object?>
                    {
                        ["SessionId"] = session.Id.Value,
                        ["RestrictionType"] = restrictionType,
                        ["RestrictionCount"] = 0
                    },
                    OperationType.Delete
                ),

                (null, [_, ..]) => throw DomainInvariantViolationException.BrokenState<User>(
                    $"Inconsistent aggregate state: restrictions for '{restrictionType}' " +
                    "exist, but session is missing.",
                    new Dictionary<string, object?>
                    {
                        ["RestrictionType"] = restrictionType,
                        ["RestrictionCount"] = restrictions.Count,
                        ["RestrictionIds"] = restrictions
                            .Select(x => x.Id.Value)
                            .ToArray()
                    },
                    OperationType.Delete
                ),

                (not null, [_, ..]) => ProceedRevocation(
                    session,
                    restrictions,
                    now
                )
            };
        }
        #endregion

        #region Session Revocation Plans
        /// <summary>
        /// Базовий план повного припинення сесії обмежень.
        ///
        /// Визначає спільні етапи вилучення сесії
        /// з активної колекції агрегату та запису її видалення.
        ///
        /// Похідні плани визначають переходи стану обмежень
        /// і відповідну доменну подію.
        ///
        /// (Base plan for completely terminating a restriction session.
        ///
        /// Defines the shared stages of removing the session
        /// from the aggregate's active collection and recording its deletion.
        ///
        /// Derived plans define restriction state transitions
        /// and the corresponding domain event.)
        /// </summary>
        private abstract class RemovalSessionPlan
        {
            /// <summary>
            /// Сесія обмежень, яку необхідно припинити та вилучити з агрегату.
            ///
            /// (Restriction session to terminate and remove from the aggregate.)
            /// </summary>
            public UserRestrictionSession Session { get; }

            protected RemovalSessionPlan(UserRestrictionSession session)
            {
                Session = session;
            }

            /// <summary>
            /// Вилучає сесію з активної колекції сесій агрегату.
            ///
            /// Похідний план викликає цей етап після зміни станів
            /// і вилучення відповідних обмежень.
            ///
            /// (Removes the session from the aggregate's active session collection.
            ///
            /// The derived plan calls this stage after changing restriction
            /// states and removing the corresponding restrictions.)
            /// </summary>
            /// <param name="user">
            /// Користувач, з активної колекції якого вилучається сесія.
            ///
            /// (User from whose active collection the session is removed.)
            /// </param>
            /// <param name="now">
            /// Час операції, переданий спільним контрактом плану.
            /// Базова реалізація не використовує цей параметр.
            ///
            /// (Operation time passed through the shared plan contract.
            /// The base implementation does not use this parameter.)
            /// </param>
            public virtual void Apply(
                User user,
                DateTimeOffset now)
            {
                user.RemoveRestrictionSession(Session);
            }

            /// <summary>
            /// Записує спільну для всіх планів зміну агрегату
            /// про видалення сесії.
            ///
            /// Похідний план викликає цей етап після запису
            /// відповідної доменної події та оновлень обмежень.
            ///
            /// (Records the session-deletion aggregate change
            /// shared by all plans.
            ///
            /// The derived plan calls this stage after recording
            /// the corresponding domain event and restriction updates.)
            /// </summary>
            /// <param name="user">
            /// Користувач, до якого додається запис про видалення сесії.
            ///
            /// (User to which the session-deletion record is added.)
            /// </param>
            /// <param name="now">
            /// Час операції, що передається до запису зміни агрегату.
            ///
            /// (Operation time passed to the aggregate-change record.)
            /// </param>
            public virtual void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddAggregateChange(new UserRestrictionSessionDeleted(
                    Session.Id,
                    now)
                );
            }
        }

        /// <summary>
        /// Представляє план повного відкликання активної сесії.
        ///
        /// Відкликає всі її обмеження та вилучає їх і сесію
        /// з активних колекцій агрегату.
        ///
        /// Окремим етапом записує доменну подію відкликання сесії,
        /// оновлення обмежень та видалення сесії.
        ///
        /// (Represents a plan for completely revoking an active session.
        ///
        /// Revokes all its restrictions and removes them and the session
        /// from the aggregate's active collections.
        ///
        /// In a separate stage, records a session-revocation domain event,
        /// restriction updates, and deletion of the session.)
        /// </summary>
        private sealed class RevokeActiveRestrictionSessionPlan : RemovalSessionPlan
        {
            /// <summary>
            /// Зафіксована колекція всіх обмежень сесії для відкликання.
            ///
            /// (Captured collection of all session restrictions to revoke.)
            /// </summary>
            public IReadOnlyCollection<UserRestriction> RestrictionsToRevoke { get; }

            public RevokeActiveRestrictionSessionPlan(
                UserRestrictionSession session,
                IReadOnlyCollection<UserRestriction> restrictionsToRevoke)
                : base(session)
            {
                RestrictionsToRevoke = restrictionsToRevoke.ToArray();
            }

            /// <summary>
            /// Відкликає всі підготовлені обмеження та вилучає їх
            /// з активної колекції обмежень агрегату.
            ///
            /// Після цього викликає базовий етап вилучення сесії.
            ///
            /// (Revokes all prepared restrictions and removes them
            /// from the aggregate's active restriction collection.
            ///
            /// Then calls the base stage to remove the session.)
            /// </summary>
            /// <param name="user">
            /// Користувач, обмеження та сесія якого вилучаються.
            ///
            /// (User whose restrictions and session are removed.)
            /// </param>
            /// <param name="now">
            /// Час, яким фіксується відкликання обмежень.
            ///
            /// (Time recorded for restriction revocation.)
            /// </param>
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

                base.Apply(
                    user,
                    now
                );
            }

            /// <summary>
            /// Записує доменну подію повного відкликання сесії
            /// та оновлення всіх відкликаних обмежень.
            ///
            /// Після цього викликає базовий етап
            /// запису видалення сесії.
            ///
            /// (Records a domain event for revoking the entire session
            /// and updates to all revoked restrictions.
            ///
            /// Then calls the base stage
            /// to record deletion of the session.)
            /// </summary>
            /// <param name="user">
            /// Користувач, до якого додаються подія та зміни агрегату.
            ///
            /// (User to which the event and aggregate changes are added.)
            /// </param>
            /// <param name="now">
            /// Час операції, що передається до події та записів змін.
            ///
            /// (Operation time passed to the event and change records.)
            /// </param>
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

                base.RecordEventsAndChanges(
                    user,
                    now
                );
            }
        }

        /// <summary>
        /// Представляє план завершення сесії із завершеним строком.
        ///
        /// Переводить усі її обмеження у завершений стан
        /// та вилучає їх і сесію з активних колекцій агрегату.
        ///
        /// Окремим етапом записує доменну подію завершення сесії,
        /// оновлення обмежень та видалення сесії.
        ///
        /// (Represents a completion plan for a session whose lifetime has ended.
        ///
        /// Marks all its restrictions as completed and removes them
        /// and the session from the aggregate's active collections.
        ///
        /// In a separate stage, records a session-completion domain event,
        /// restriction updates, and deletion of the session.)
        /// </summary>
        private sealed class CompleteExpiredRestrictionSessionPlan : RemovalSessionPlan
        {
            /// <summary>
            /// Зафіксована колекція всіх обмежень
            /// простроченої сесії для завершення.
            ///
            /// (Captured collection of all restrictions
            /// in the expired session to complete.)
            /// </summary>
            public IReadOnlyCollection<UserRestriction> RestrictionsToComplete { get; }

            public CompleteExpiredRestrictionSessionPlan(
                UserRestrictionSession session,
                IReadOnlyCollection<UserRestriction> restrictionsToComplete)
                : base(session)
            {
                RestrictionsToComplete = restrictionsToComplete.ToArray();
            }

            /// <summary>
            /// Завершує всі підготовлені обмеження та вилучає їх
            /// з активної колекції обмежень агрегату.
            ///
            /// Після цього викликає базовий етап вилучення сесії.
            ///
            /// (Completes all prepared restrictions and removes them
            /// from the aggregate's active restriction collection.
            ///
            /// Then calls the base stage to remove the session.)
            /// </summary>
            /// <param name="user">
            /// Користувач, обмеження та сесія якого вилучаються.
            ///
            /// (User whose restrictions and session are removed.)
            /// </param>
            /// <param name="now">
            /// Час, яким фіксується завершення обмежень.
            ///
            /// (Time recorded for restriction completion.)
            /// </param>
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

                base.Apply(
                    user,
                    now
                );
            }

            /// <summary>
            /// Записує доменну подію завершення сесії
            /// та оновлення всіх завершених обмежень.
            ///
            /// Після цього викликає базовий етап
            /// запису видалення сесії.
            ///
            /// (Records a session-completion domain event
            /// and updates to all completed restrictions.
            ///
            /// Then calls the base stage
            /// to record deletion of the session.)
            /// </summary>
            /// <param name="user">
            /// Користувач, до якого додаються подія та зміни агрегату.
            ///
            /// (User to which the event and aggregate changes are added.)
            /// </param>
            /// <param name="now">
            /// Час операції, що передається до події та записів змін.
            ///
            /// (Operation time passed to the event and change records.)
            /// </param>
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

                base.RecordEventsAndChanges(
                    user,
                    now
                );
            }
        }
        #endregion

        #region Session Revocation Orchestration
        /// <summary>
        /// Координує повне припинення знайденої сесії обмежень.
        ///
        /// Спочатку готує та перевіряє план операції.
        /// Якщо підготовка повертає помилку, план не застосовується.
        ///
        /// Після успішної підготовки застосовує план
        /// і записує відповідні доменні події та зміни агрегату.
        ///
        /// (Coordinates complete termination of an existing restriction session.
        ///
        /// First prepares and validates the operation plan.
        /// If preparation returns a failure, the plan is not applied.
        ///
        /// After successful preparation, applies the plan
        /// and records the corresponding domain events and aggregate changes.)
        /// </summary>
        /// <param name="session">
        /// Знайдена сесія, яку необхідно припинити.
        ///
        /// (Existing session to terminate.)
        /// </param>
        /// <param name="restrictions">
        /// Усі активні обмеження відповідного типу, знайдені в агрегаті.
        ///
        /// (All active restrictions of the corresponding type found in the aggregate.)
        /// </param>
        /// <param name="now">
        /// Фіксований час підготовки, застосування та запису наслідків плану.
        ///
        /// (Fixed time used for preparing, applying, and recording the plan's consequences.)
        /// </param>
        /// <returns>
        /// Успіх після застосування плану;
        /// інакше — доменна помилка його підготовки.
        ///
        /// (Success after applying the plan;
        /// otherwise, the domain error from its preparation.)
        /// </returns>
        private Result ProceedRevocation(
            UserRestrictionSession session,
            IReadOnlyCollection<UserRestriction> restrictions,
            DateTimeOffset now)
        {
            var preparationResult = PrepareRestrictionSessionRevocationPlan(
                session,
                restrictions,
                now
            );

            if (preparationResult.IsFailure)
                return Result.Failure(preparationResult.Errors);

            var plan = preparationResult.Value;

            plan.Apply(
                this,
                now
            );

            plan.RecordEventsAndChanges(
                this,
                now
            );

            UpdatedAt = now;

            return Result.Success();
        }
        #endregion

        #region Session Revocation Plan Preparation
        /// <summary>
        /// Перевіряє точну відповідність сесії переданим обмеженням
        /// і готує план її повного припинення.
        ///
        /// Для сесії із завершеним строком готує план завершення обмежень.
        /// Для активної сесії, яка стане порожньою після їх вилучення,
        /// готує план повного відкликання.
        ///
        /// Оскільки передається повний набір обмежень сесії,
        /// наслідки часткового зняття вважаються порушенням інваріанта.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Verifies an exact match between the session and supplied restrictions
        /// and prepares a plan for its complete termination.
        ///
        /// For a session whose lifetime has ended, prepares a restriction-completion plan.
        /// For an active session that becomes empty after removal,
        /// prepares a complete revocation plan.
        ///
        /// Because all session restrictions are supplied,
        /// partial-removal effects are treated as an invariant violation.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="session">
        /// Сесія, для якої готується план повного припинення.
        ///
        /// (Session for which a complete termination plan is prepared.)
        /// </param>
        /// <param name="restrictions">
        /// Повний набір активних обмежень відповідного типу з агрегату.
        ///
        /// (Complete set of active restrictions of the corresponding type from the aggregate.)
        /// </param>
        /// <param name="now">
        /// Фіксований час, відносно якого перевіряється наслідок вилучення.
        ///
        /// (Fixed time used to evaluate the removal effect.)
        /// </param>
        /// <returns>
        /// Підготовлений план; інакше — доменна помилка
        /// перевірки відкликання обмежень.
        ///
        /// (Prepared plan; otherwise, a domain error
        /// from restriction-revocation validation.)
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо набори ідентифікаторів сесії та обмежень
        /// не збігаються або повне вилучення повернуло наслідок часткового зняття.
        ///
        /// (Thrown when the session and restriction identifier sets
        /// do not match, or full removal produces a partial-removal effect.)
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо повернуто непідтримуваний результат перевірки вилучення.
        ///
        /// (Thrown when removal validation returns an unsupported result.)
        /// </exception>
        private Result<RemovalSessionPlan> PrepareRestrictionSessionRevocationPlan(
            UserRestrictionSession session,
            IReadOnlyCollection<UserRestriction> restrictions,
            DateTimeOffset now)
        {
            var restrictionIds = restrictions
                .Select(x => x.Id)
                .ToArray();

            if (!session.ContainsExactly(restrictionIds))
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "The restriction session does not exactly match the active restrictions of its type.",
                    new Dictionary<string, object?>
                    {
                        ["SessionId"] = session.Id.Value,
                        ["RestrictionType"] = session.RestrictionType,
                        ["AggregateRestrictionCount"] = restrictionIds.Length,
                        ["AggregateRestrictionIds"] = restrictionIds
                            .Select(x => x.Value)
                            .ToArray(),
                        ["SessionRestrictionIds"] = session.ActiveRestrictionIds
                            .Select(x => x.Value)
                            .ToArray()
                    },
                    OperationType.Delete
                );
            }

            var removalEffect = session.ValidateCanRemoveRestrictions(
                restrictions,
                now
            );

            return removalEffect switch
            {
                RestrictionSessionRemovalEffect.AlreadyExpired =>
                    PrepareExpiredSessionCompletionPlan(
                        session,
                        restrictions,
                        now
                    ),

                RestrictionSessionRemovalEffect.BecomesEmpty =>
                    PrepareActiveSessionRevocationPlan(
                        session,
                        restrictions,
                        now
                    ),

                RestrictionSessionRemovalEffect.BecomesExpired or
                RestrictionSessionRemovalEffect.RemainsActive =>
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "Removing all restrictions from a session produced " +
                        "an unexpected removal effect.",
                        new Dictionary<string, object?>
                        {
                            ["SessionId"] = session.Id.Value,
                            ["RestrictionType"] = session.RestrictionType,
                            ["RemovalEffect"] = removalEffect,
                            ["RestrictionCount"] = restrictions.Count
                        },
                        OperationType.Delete
                    ),

                _ => throw DomainDataInconsistencyException.UnsupportedDiscriminator<User>(
                    nameof(removalEffect),
                    removalEffect,
                    OperationType.Delete
                )
            };
        }

        /// <summary>
        /// Перевіряє можливість завершення всіх обмежень
        /// простроченої сесії та готує відповідний план.
        ///
        /// Створює план лише після перевірки кожного обмеження.
        /// Підготовка не змінює стан агрегату.
        ///
        /// (Validates that all restrictions of an expired session
        /// can be completed and prepares the corresponding plan.
        ///
        /// Creates the plan only after validating every restriction.
        /// Preparation does not modify aggregate state.)
        /// </summary>
        /// <param name="session">
        /// Сесія із завершеним строком, яку необхідно припинити.
        ///
        /// (Session whose lifetime has ended and which must be terminated.)
        /// </param>
        /// <param name="restrictions">
        /// Усі обмеження сесії, які необхідно перевести у завершений стан.
        ///
        /// (All session restrictions to mark as completed.)
        /// </param>
        /// <param name="now">
        /// Фіксований час перевірки можливості завершення.
        ///
        /// (Fixed time used to validate completion.)
        /// </param>
        /// <returns>
        /// Успішний результат із підготовленим планом завершення сесії.
        ///
        /// (Successful result containing the prepared session-completion plan.)
        /// </returns>
        private Result<RemovalSessionPlan> PrepareExpiredSessionCompletionPlan(
            UserRestrictionSession session,
            IReadOnlyCollection<UserRestriction> restrictions,
            DateTimeOffset now)
        {
            foreach (var restriction in restrictions)
            {
                restriction.ValidateCanComplete(now);
            }

            RemovalSessionPlan plan = new CompleteExpiredRestrictionSessionPlan(
                session,
                restrictions
            );

            return Result<RemovalSessionPlan>.Success(plan);
        }

        /// <summary>
        /// Перевіряє можливість відкликання всіх обмежень
        /// активної сесії та готує відповідний план.
        ///
        /// Повертає помилку першого обмеження, яке не може бути відкликане.
        /// Створює план лише після успішної перевірки всіх обмежень.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Validates that all restrictions of an active session
        /// can be revoked and prepares the corresponding plan.
        ///
        /// Returns the first validation failure for a restriction
        /// that cannot be revoked. Creates the plan only after
        /// all restrictions have been successfully validated.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="session">
        /// Активна сесія, яку необхідно повністю відкликати.
        ///
        /// (Active session to revoke entirely.)
        /// </param>
        /// <param name="restrictions">
        /// Усі обмеження сесії, які необхідно відкликати.
        ///
        /// (All session restrictions to revoke.)
        /// </param>
        /// <param name="now">
        /// Фіксований час перевірки можливості відкликання.
        ///
        /// (Fixed time used to validate revocation.)
        /// </param>
        /// <returns>
        /// Підготовлений план; інакше — перша доменна помилка
        /// перевірки відкликання обмежень.
        ///
        /// (Prepared plan; otherwise, the first domain error
        /// from restriction-revocation validation.)
        /// </returns>
        private Result<RemovalSessionPlan> PrepareActiveSessionRevocationPlan(
            UserRestrictionSession session,
            IReadOnlyCollection<UserRestriction> restrictions,
            DateTimeOffset now)
        {
            foreach (var restriction in restrictions)
            {
                var validationResult = restriction.ValidateRevocation(now);

                if (validationResult.IsFailure)
                    return Result<RemovalSessionPlan>.Failure(validationResult.Errors);
            }

            RemovalSessionPlan plan = new RevokeActiveRestrictionSessionPlan(
                session,
                restrictions
            );

            return Result<RemovalSessionPlan>.Success(plan);
        }
        #endregion
    }
}
