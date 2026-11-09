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

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        #region RemoveRestrictions
        /// <summary>
        /// Знімає одне або кілька активних обмежень користувача.
        ///
        /// Перевіряє передані ідентифікатори, групує знайдені обмеження
        /// за типом і готує план для кожної відповідної сесії.
        /// Очікувані помилки відбору та підготовки повертаються
        /// до початку застосування планів.
        ///
        /// Після успішної підготовки всіх планів послідовно застосовує
        /// переходи обмежень і сесій, а потім записує доменні події
        /// та зміни агрегату.
        ///
        /// Якщо сесія вже завершилася за часом, завершує всі її активні
        /// обмеження. Якщо вона завершується внаслідок зняття вибраних
        /// обмежень, відкликає вибрані та завершує решту.
        /// Зняття всіх обмежень активної сесії скасовує її повністю.
        ///
        /// (Removes one or more active user restrictions.
        ///
        /// Validates the supplied identifiers, groups matching restrictions
        /// by type, and prepares a plan for each corresponding session.
        /// Expected selection and preparation failures are returned
        /// before any plans are applied.
        ///
        /// After all plans have been successfully prepared, sequentially
        /// applies restriction and session transitions, then records
        /// domain events and aggregate changes.
        ///
        /// If a session's lifetime has already ended, completes all its active
        /// restrictions. If removing the selected restrictions causes the session
        /// to expire, revokes the selected restrictions and completes the rest.
        /// Removing all restrictions from an active session revokes it entirely.)
        /// </summary>
        /// <param name="restrictionIds">
        /// Ідентифікатори обмежень, які необхідно зняти.
        ///
        /// (Identifiers of the restrictions to remove.)
        /// </param>
        /// <param name="now">
        /// Фіксований час усієї операції, який не може передувати
        /// часу створення користувача.
        ///
        /// (Fixed time for the entire operation, which cannot precede
        /// the user's creation time.)
        /// </param>
        /// <returns>
        /// Успіх після застосування планів; інакше — доменна помилка,
        /// якщо колекція порожня, містить дублікати, обмеження не знайдені
        /// або вибрані обмеження не можуть бути відкликані.
        ///
        /// (Success after applying the plans; otherwise, a domain error
        /// when the collection is empty, contains duplicates, restrictions
        /// are missing, or selected restrictions cannot be revoked.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо колекція містить null-ідентифікатори.
        ///
        /// (Thrown when the collection contains null identifiers.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження не мають відповідної сесії,
        /// її склад не збігається зі станом агрегату або повторна
        /// перевірка повного зняття повертає неочікуваний результат.
        ///
        /// (Thrown when restrictions have no corresponding session,
        /// its contents do not match aggregate state, or repeated
        /// full-removal validation produces an unexpected result.)
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо сесія повернула непідтримуваний
        /// результат перевірки наслідків зняття обмежень.
        ///
        /// (Thrown when a session returns an unsupported result
        /// from restriction-removal effect validation.)
        /// </exception>
        public Result RemoveRestrictions(
            IEnumerable<UserRestrictionId> restrictionIds,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (restrictionIds, nameof(restrictionIds))
            );

            EnsureValidOperationTimestamp(now);

            var requestedRestrictionIds = restrictionIds.ToList();

            if (requestedRestrictionIds.Count == 0)
            {
                return Result.Failure(UserErrors.EmptyRestrictionList<User>());
            }

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
            {
                return Result.Failure(UserErrors.DuplicateRestrictionIdentifiers<User>());
            }

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
            {
                return Result.Failure(UserErrors.NotFoundRestriction<User>(missingRestrictionIds));
            }

            var removalPlansResult = PrepareRestrictionRemovalPlans(
                matchedRestrictions,
                now
            );

            if (removalPlansResult.IsFailure)
            {
                return Result.Failure(removalPlansResult.Errors);
            }

            ApplyRestrictionRemovalPlans(
                removalPlansResult.Value,
                now
            );

            RecordRestrictionRemovalEventsAndChanges(
                removalPlansResult.Value,
                now
            );

            UpdatedAt = now;

            return Result.Success();
        }
        #endregion

        #region Removal Plans
        /// <summary>
        /// Представляє виконуваний план зняття обмежень
        /// у межах однієї сесії.
        ///
        /// Визначає окремі етапи застосування доменного стану
        /// та запису доменних подій і змін агрегату.
        /// Створення плану не змінює стан агрегату.
        ///
        /// (Represents an executable restriction-removal plan
        /// within a single session.
        ///
        /// Defines separate stages for applying domain state
        /// and recording domain events and aggregate changes.
        /// Creating the plan does not modify aggregate state.)
        /// </summary>
        private abstract class RestrictionRemovalPlan
        {
            /// <summary>
            /// Сесія обмежень, для якої підготовлено план.
            ///
            /// (Restriction session for which the plan was prepared.)
            /// </summary>
            public UserRestrictionSession Session { get; }

            protected RestrictionRemovalPlan(UserRestrictionSession session)
            {
                Session = session;
            }

            /// <summary>
            /// Застосовує попередньо перевірені переходи
            /// доменного стану, визначені планом.
            ///
            /// Повинен викликатися лише після успішної
            /// підготовки всіх планів операції.
            ///
            /// (Applies the pre-validated domain-state transitions
            /// defined by the plan.
            ///
            /// Must be called only after all operation plans
            /// have been successfully prepared.)
            /// </summary>
            /// <param name="user">
            /// Агрегат користувача, до якого застосовується план.
            ///
            /// (User aggregate to which the plan is applied.)
            /// </param>
            /// <param name="now">
            /// Фіксований час виконання операції.
            ///
            /// (Fixed operation time.)
            /// </param>
            public abstract void Apply(
                User user,
                DateTimeOffset now);

            /// <summary>
            /// Записує доменні події та зміни агрегату,
            /// визначені вже застосованим планом.
            ///
            /// Не виконує повторних переходів доменного стану.
            ///
            /// (Records domain events and aggregate changes
            /// defined by a plan that has already been applied.
            ///
            /// Does not repeat domain-state transitions.)
            /// </summary>
            /// <param name="user">
            /// Агрегат користувача, до якого додаються події та зміни.
            ///
            /// (User aggregate to which events and changes are added.)
            /// </param>
            /// <param name="now">
            /// Час операції, що передається до подій і записів змін.
            ///
            /// (Operation time passed to events and change records.)
            /// </param>
            public abstract void RecordEventsAndChanges(
                User user,
                DateTimeOffset now);
        }

        /// <summary>
        /// Представляє план відкликання частини обмежень,
        /// після якого сесія залишається активною.
        ///
        /// Вибрані обмеження відкликаються та вилучаються,
        /// а сесія оновлюється відповідно до обмежень, що залишилися.
        ///
        /// (Represents a plan for revoking some restrictions
        /// while the session remains active.
        ///
        /// Selected restrictions are revoked and removed,
        /// and the session is updated according to the remaining restrictions.)
        /// </summary>
        private sealed class SessionRemainsActivePlan : RestrictionRemovalPlan
        {
            /// <summary>
            /// Зафіксована колекція вибраних обмежень для відкликання.
            ///
            /// (Captured collection of selected restrictions to revoke.)
            /// </summary>
            public IReadOnlyCollection<UserRestriction> RestrictionsToRevoke { get; }

            public SessionRemainsActivePlan(
                UserRestrictionSession session,
                IReadOnlyCollection<UserRestriction> restrictionsToRevoke)
                : base(session)
            {
                RestrictionsToRevoke = restrictionsToRevoke.ToArray();
            }

            /// <summary>
            /// Відкликає вибрані обмеження та видаляє їх
            /// з активної колекції обмежень агрегату.
            ///
            /// Після цього вилучає їх із сесії,
            /// яка залишається в активній колекції сесій.
            ///
            /// (Revokes the selected restrictions and removes them
            /// from the aggregate's active restriction collection.
            ///
            /// Then removes them from the session,
            /// which remains in the active session collection.)
            /// </summary>
            /// <param name="user">
            /// Користувач, обмеження якого відкликаються.
            ///
            /// (User whose restrictions are revoked.)
            /// </param>
            /// <param name="now">
            /// Час відкликання обмежень та оновлення сесії.
            ///
            /// (Time used for revoking restrictions and updating the session.)
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

                Session.RemoveRestrictions(
                    RestrictionsToRevoke,
                    now
                );
            }

            /// <summary>
            /// Записує доменну подію зняття обмежень із сесії,
            /// оновлення відкликаних обмежень та оновлення сесії.
            ///
            /// Подія містить загальну тривалість уже оновленої сесії.
            ///
            /// (Records a domain event for removing restrictions from the session,
            /// updates to revoked restrictions, and an update to the session.
            ///
            /// The event includes the updated session's total duration.)
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
        /// Представляє план завершення сесії,
        /// строк якої вже завершився на момент операції.
        ///
        /// Завершує всі її активні обмеження,
        /// очищає склад сесії та видаляє її з агрегату.
        ///
        /// (Represents a plan for completing a session
        /// whose lifetime had already ended at the operation time.
        ///
        /// Completes all its active restrictions,
        /// clears the session's contents, and removes it from the aggregate.)
        /// </summary>
        private sealed class SessionAlreadyExpiredPlan : RestrictionRemovalPlan
        {
            /// <summary>
            /// Зафіксована колекція всіх активних обмежень
            /// простроченої сесії для завершення.
            ///
            /// (Captured collection of all active restrictions
            /// in the expired session to complete.)
            /// </summary>
            public IReadOnlyCollection<UserRestriction> RestrictionsToComplete { get; }

            public SessionAlreadyExpiredPlan(
                UserRestrictionSession session,
                IReadOnlyCollection<UserRestriction> restrictionsToComplete)
                : base(session)
            {
                RestrictionsToComplete = restrictionsToComplete.ToArray();
            }

            /// <summary>
            /// Завершує всі підготовлені обмеження сесії
            /// та видаляє їх з активної колекції агрегату.
            ///
            /// Після цього очищає склад сесії
            /// та видаляє її з активної колекції сесій.
            ///
            /// (Completes all prepared session restrictions
            /// and removes them from the aggregate's active collection.
            ///
            /// Then clears the session's contents
            /// and removes it from the active session collection.)
            /// </summary>
            /// <param name="user">
            /// Користувач, прострочена сесія якого обробляється.
            ///
            /// (User whose expired session is processed.)
            /// </param>
            /// <param name="now">
            /// Час завершення обмежень та очищення сесії.
            ///
            /// (Time used for completing restrictions and clearing the session.)
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

                Session.RemoveRestrictions(
                    RestrictionsToComplete,
                    now
                );

                user.RemoveRestrictionSession(Session);
            }

            /// <summary>
            /// Записує доменну подію завершення сесії,
            /// оновлення завершених обмежень та видалення сесії.
            ///
            /// (Records a session-completion domain event,
            /// updates to completed restrictions, and deletion of the session.)
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

                user.AddAggregateChange(new UserRestrictionSessionDeleted(
                    Session.Id,
                    now)
                );
            }
        }

        /// <summary>
        /// Представляє план для сесії, строк якої завершується
        /// внаслідок зняття вибраних обмежень.
        ///
        /// Вибрані обмеження відкликаються, решта завершуються,
        /// після чого сесія очищається та видаляється з агрегату.
        ///
        /// (Represents a plan for a session whose lifetime ends
        /// as a result of removing selected restrictions.
        ///
        /// Selected restrictions are revoked and the rest are completed,
        /// after which the session is cleared and removed from the aggregate.)
        /// </summary>
        private sealed class SessionBecomesExpiredPlan : RestrictionRemovalPlan
        {
            /// <summary>
            /// Зафіксована колекція вибраних обмежень для відкликання.
            ///
            /// (Captured collection of selected restrictions to revoke.)
            /// </summary>
            public IReadOnlyCollection<UserRestriction> RestrictionsToRevoke { get; }

            /// <summary>
            /// Зафіксована колекція решти обмежень сесії для завершення.
            ///
            /// (Captured collection of the remaining session restrictions to complete.)
            /// </summary>
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

            /// <summary>
            /// Відкликає вибрані обмеження та завершує решту,
            /// видаляючи обидві групи з активної колекції агрегату.
            ///
            /// Після цього вилучає всі оброблені обмеження із сесії
            /// та видаляє її з активної колекції сесій.
            ///
            /// (Revokes selected restrictions and completes the rest,
            /// removing both groups from the aggregate's active collection.
            ///
            /// Then removes all processed restrictions from the session
            /// and removes it from the active session collection.)
            /// </summary>
            /// <param name="user">
            /// Користувач, обмеження та сесія якого обробляються.
            ///
            /// (User whose restrictions and session are processed.)
            /// </param>
            /// <param name="now">
            /// Час відкликання, завершення обмежень та очищення сесії.
            ///
            /// (Time used for revocation, restriction completion, and clearing the session.)
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

            /// <summary>
            /// Записує доменну подію завершення сесії,
            /// оновлення відкликаних і завершених обмежень
            /// та видалення сесії.
            ///
            /// (Records a session-completion domain event,
            /// updates to revoked and completed restrictions,
            /// and deletion of the session.)
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
        /// Представляє план відкликання всіх обмежень активної сесії.
        ///
        /// Після відкликання обмежень сесія очищається,
        /// видаляється з агрегату та фіксується як повністю скасована
        /// відповідною доменною подією.
        ///
        /// (Represents a plan for revoking all restrictions in an active session.
        ///
        /// After revoking the restrictions, the session is cleared,
        /// removed from the aggregate, and reported as entirely revoked
        /// through the corresponding domain event.)
        /// </summary>
        private sealed class SessionBecomesEmptyPlan : RestrictionRemovalPlan
        {
            /// <summary>
            /// Зафіксована колекція всіх обмежень сесії для відкликання.
            ///
            /// (Captured collection of all session restrictions to revoke.)
            /// </summary>
            public IReadOnlyCollection<UserRestriction> RestrictionsToRevoke { get; }

            public SessionBecomesEmptyPlan(
                UserRestrictionSession session,
                IReadOnlyCollection<UserRestriction> restrictionsToRevoke)
                : base(session)
            {
                RestrictionsToRevoke = restrictionsToRevoke.ToArray();
            }

            /// <summary>
            /// Відкликає всі підготовлені обмеження сесії
            /// та видаляє їх з активної колекції агрегату.
            ///
            /// Після цього очищає склад сесії
            /// та видаляє її з активної колекції сесій.
            ///
            /// (Revokes all prepared session restrictions
            /// and removes them from the aggregate's active collection.
            ///
            /// Then clears the session's contents
            /// and removes it from the active session collection.)
            /// </summary>
            /// <param name="user">
            /// Користувач, обмеження сесії якого відкликаються.
            ///
            /// (User whose session restrictions are revoked.)
            /// </param>
            /// <param name="now">
            /// Час відкликання обмежень та очищення сесії.
            ///
            /// (Time used for revoking restrictions and clearing the session.)
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

                Session.RemoveRestrictions(
                    RestrictionsToRevoke,
                    now
                );

                user.RemoveRestrictionSession(Session);
            }

            /// <summary>
            /// Записує доменну подію повного скасування сесії,
            /// оновлення відкликаних обмежень та видалення сесії.
            ///
            /// (Records a domain event for revoking the entire session,
            /// updates to revoked restrictions, and deletion of the session.)
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
        /// Перевіряє існування сесії та точний збіг її ідентифікаторів
        /// з активними обмеженнями агрегату відповідного типу.
        ///
        /// Визначає наслідок зняття вибраних обмежень і готує
        /// план збереження активної сесії, завершення простроченої,
        /// завершення сесії після зняття частини обмежень
        /// або повного відкликання її обмежень.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Groups requested restrictions by type and prepares
        /// an executable plan for each corresponding session.
        ///
        /// Verifies session existence and an exact match between its
        /// restriction identifiers and the aggregate's active restrictions
        /// of the corresponding type.
        ///
        /// Determines the effect of removing selected restrictions and prepares
        /// a plan for retaining an active session, completing an expired session,
        /// completing a session after partial removal,
        /// or revoking all its restrictions.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="requestedRestrictions">
        /// Знайдені в агрегаті обмеження, які запитано зняти.
        ///
        /// (Restrictions found in the aggregate and requested for removal.)
        /// </param>
        /// <param name="now">
        /// Фіксований час перевірок для всіх груп обмежень.
        ///
        /// (Fixed validation time for all restriction groups.)
        /// </param>
        /// <returns>
        /// Підготовлені плани; інакше — доменна помилка,
        /// якщо вибране обмеження не може бути відкликане.
        ///
        /// (Prepared plans; otherwise, a domain error
        /// when a selected restriction cannot be revoked.)
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо активні обмеження не мають відповідної сесії,
        /// склад сесії не збігається зі станом агрегату або підготовка
        /// плану виявила неочікуваний наслідок повного зняття.
        ///
        /// (Thrown when active restrictions have no corresponding session,
        /// session contents do not match aggregate state, or plan preparation
        /// detects an unexpected full-removal effect.)
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо сесія повернула непідтримуваний наслідок зняття обмежень.
        ///
        /// (Thrown when the session returns an unsupported restriction-removal effect.)
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
                {
                    return Result<IReadOnlyCollection<RestrictionRemovalPlan>>.Failure(planResult.Errors);
                }

                plans.Add(planResult.Value);
            }

            return Result<IReadOnlyCollection<RestrictionRemovalPlan>>.Success(plans.AsReadOnly());
        }

        /// <summary>
        /// Готує план завершення сесії із завершеним строком.
        ///
        /// Знаходить усі активні обмеження її типу,
        /// перевіряє можливість завершення кожного та повторно
        /// перевіряє наслідок їх повного вилучення із сесії.
        ///
        /// Очікує результат перевірки, що відповідає
        /// вже простроченій сесії. Стан агрегату не змінюється.
        ///
        /// (Prepares a completion plan for a session whose lifetime has ended.
        ///
        /// Finds all active restrictions of its type, validates
        /// that each can be completed, and revalidates the effect
        /// of removing all of them from the session.
        ///
        /// Expects the validation result for an already expired session.
        /// Aggregate state remains unchanged.)
        /// </summary>
        /// <param name="restrictionSession">
        /// Сесія із завершеним строком, яку необхідно обробити.
        ///
        /// (Session whose lifetime has ended and which must be processed.)
        /// </param>
        /// <param name="now">
        /// Фіксований час перевірки можливості завершення обмежень.
        ///
        /// (Fixed time used to validate restriction completion.)
        /// </param>
        /// <returns>
        /// Успішний результат із підготовленим планом завершення сесії.
        ///
        /// (Successful result containing the prepared session-completion plan.)
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо повторна перевірка повного вилучення
        /// не повернула результат для вже простроченої сесії.
        ///
        /// (Thrown when repeated full-removal validation
        /// does not return the result for an already expired session.)
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
        /// Готує план для сесії, строк якої завершується
        /// після зняття вибраних обмежень.
        ///
        /// Перевіряє можливість відкликання вибраних обмежень
        /// та завершення решти активних обмежень її типу.
        ///
        /// Додатково перевіряє, що вилучення обох груп
        /// повністю спорожнить сесію.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Prepares a plan for a session whose lifetime ends
        /// after removing selected restrictions.
        ///
        /// Validates revocation of the selected restrictions
        /// and completion of the remaining active restrictions of its type.
        ///
        /// Also verifies that removing both groups
        /// will leave the session empty.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="restrictionSession">
        /// Сесія, строк якої завершується після зняття вибраних обмежень.
        ///
        /// (Session whose lifetime ends after removing the selected restrictions.)
        /// </param>
        /// <param name="restrictionsToRevoke">
        /// Вибрані обмеження, які необхідно відкликати.
        ///
        /// (Selected restrictions to revoke.)
        /// </param>
        /// <param name="now">
        /// Фіксований час перевірки відкликання та завершення обмежень.
        ///
        /// (Fixed time used to validate restriction revocation and completion.)
        /// </param>
        /// <returns>
        /// Підготовлений план; інакше — доменна помилка,
        /// якщо вибране обмеження не може бути відкликане.
        ///
        /// (Prepared plan; otherwise, a domain error
        /// when a selected restriction cannot be revoked.)
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо перевірка вилучення всіх обмежень
        /// не повернула результат порожньої сесії.
        ///
        /// (Thrown when validation of removing all restrictions
        /// does not return the empty-session result.)
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
                {
                    return Result<RestrictionRemovalPlan>.Failure(revocationValidationResult.Errors);
                }
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
        /// Перевіряє можливість відкликання кожного обмеження.
        /// Стан агрегату під час підготовки не змінюється.
        ///
        /// (Prepares a plan for revoking all session restrictions,
        /// leaving the session empty.
        ///
        /// Validates that each restriction can be revoked.
        /// Aggregate state remains unchanged during preparation.)
        /// </summary>
        /// <param name="restrictionSession">
        /// Сесія, яка стане порожньою після відкликання обмежень.
        ///
        /// (Session that will become empty after revoking its restrictions.)
        /// </param>
        /// <param name="restrictionsToRevoke">
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
        /// Підготовлений план; інакше — доменна помилка,
        /// якщо обмеження не може бути відкликане.
        ///
        /// (Prepared plan; otherwise, a domain error
        /// when a restriction cannot be revoked.)
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
                {
                    return Result<RestrictionRemovalPlan>.Failure(revocationValidationResult.Errors);
                }
            }

            var plan = new SessionBecomesEmptyPlan(
                restrictionSession,
                restrictionsToRevoke
            );

            return Result<RestrictionRemovalPlan>.Success(plan);
        }

        /// <summary>
        /// Готує план відкликання частини обмежень,
        /// після якого сесія залишиться активною.
        ///
        /// Перевіряє можливість відкликання кожного вибраного обмеження.
        /// Стан агрегату під час підготовки не змінюється.
        ///
        /// (Prepares a plan for revoking some restrictions
        /// while the session remains active.
        ///
        /// Validates that each selected restriction can be revoked.
        /// Aggregate state remains unchanged during preparation.)
        /// </summary>
        /// <param name="restrictionSession">
        /// Сесія, яка залишиться активною після зняття обмежень.
        ///
        /// (Session that will remain active after removing the restrictions.)
        /// </param>
        /// <param name="restrictionsToRevoke">
        /// Вибрані обмеження, які необхідно відкликати.
        ///
        /// (Selected restrictions to revoke.)
        /// </param>
        /// <param name="now">
        /// Фіксований час перевірки можливості відкликання.
        ///
        /// (Fixed time used to validate revocation.)
        /// </param>
        /// <returns>
        /// Підготовлений план; інакше — доменна помилка,
        /// якщо вибране обмеження не може бути відкликане.
        ///
        /// (Prepared plan; otherwise, a domain error
        /// when a selected restriction cannot be revoked.)
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
                {
                    return Result<RestrictionRemovalPlan>.Failure(revocationValidationResult.Errors);
                }
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
        /// Послідовно застосовує всі підготовлені
        /// плани зняття обмежень.
        ///
        /// Делегує кожному плану відповідні переходи
        /// стану обмежень та сесії.
        ///
        /// Повинен викликатися лише після успішної
        /// підготовки та перевірки всіх планів операції.
        ///
        /// (Sequentially applies all prepared restriction-removal plans.
        ///
        /// Delegates the corresponding restriction and session
        /// state transitions to each plan.
        ///
        /// Must be called only after all operation plans
        /// have been successfully prepared and validated.)
        /// </summary>
        /// <param name="removalPlans">
        /// Підготовлені та перевірені плани зняття обмежень.
        ///
        /// (Prepared and validated restriction-removal plans.)
        /// </param>
        /// <param name="now">
        /// Фіксований час застосування всіх планів.
        ///
        /// (Fixed time used for applying all plans.)
        /// </param>
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
        /// Записує доменні події та зміни агрегату
        /// для всіх уже застосованих планів зняття обмежень.
        ///
        /// Делегує кожному плану запис наслідків
        /// відповідного доменного сценарію.
        ///
        /// Не виконує повторних переходів доменного стану.
        ///
        /// (Records domain events and aggregate changes
        /// for all restriction-removal plans that have already been applied.
        ///
        /// Delegates recording the consequences
        /// of the corresponding domain scenario to each plan.
        ///
        /// Does not repeat domain-state transitions.)
        /// </summary>
        /// <param name="removalPlans">
        /// Застосовані плани зняття обмежень.
        ///
        /// (Applied restriction-removal plans.)
        /// </param>
        /// <param name="now">
        /// Час операції, що передається до подій і записів змін.
        ///
        /// (Operation time passed to events and change records.)
        /// </param>
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
