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

namespace Authorization.Domain.Users
{
    public partial class User
    {
        #region Revoke Restriction Session
        /// <summary>
        /// Повністю припиняє сесію обмежень указаного типу.
        ///
        /// Якщо сесія ще активна, усі її обмеження відкликаються.
        /// Якщо сесія вже завершилася за часом, усі її обмеження
        /// переводяться у стан завершених.
        ///
        /// Перед зміною агрегату перевіряє відповідність сесії
        /// та її активних обмежень і готує план операції.
        /// Після успішної підготовки застосовує зміни, видаляє
        /// сесію та записує доменну подію і зміни агрегату.
        ///
        /// (Completely terminates a restriction session of the specified type.
        ///
        /// If the session is still active, all its restrictions are revoked.
        /// If the session has already expired, all its restrictions are marked
        /// as completed.
        ///
        /// Before mutating the aggregate, verifies that the session matches
        /// its active restrictions and prepares the appropriate operation plan.
        /// After successful preparation, applies the changes, removes the session,
        /// and records the domain event and aggregate changes.)
        /// </summary>
        /// <param name="restrictionType">Тип сесії обмежень, яку необхідно припинити.</param>
        /// <param name="now">Зафіксований час виконання операції.</param>
        /// <returns>
        /// Успішний результат або помилка, якщо сесію не знайдено
        /// чи її активні обмеження неможливо відкликати.
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо сесія та обмеження агрегату не відповідають
        /// одне одному або отримано неможливий результат їх вилучення.
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо отримано непідтримуваний результат
        /// перевірки вилучення обмежень.
        /// </exception>
        public Result RevokeRestrictionSession(
            RestrictionType restrictionType,
            DateTimeOffset now) 
        {
            DomainGuard.AgainstUndefinedEnum<User>(
                OperationType.Update,
                (restrictionType, nameof(restrictionType))
            );

            DomainGuard.AgainstEarlierThan<User>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

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
                    OperationType.Update
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
                    OperationType.Update
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
        /// Містить спільні для всіх сценаріїв дії:
        /// вилучення сесії зі стану користувача та запис
        /// зміни агрегату про її видалення.
        ///
        /// Похідні плани визначають спосіб зміни статусів
        /// обмежень і відповідну доменну подію.
        ///
        /// (Base plan for completely terminating a restriction session.
        ///
        /// Contains the operations shared by all scenarios:
        /// removing the session from the user state and recording
        /// its deletion as an aggregate change.
        ///
        /// Derived plans determine how restriction statuses are changed
        /// and which domain event is recorded.)
        /// </summary>
        private abstract class RemovalSessionPlan
        {
            public UserRestrictionSession Session { get; }

            protected RemovalSessionPlan(UserRestrictionSession session)
            {
                Session = session;
            }

            /// <summary>
            /// Виконує спільну мутацію для всіх планів,
            /// вилучаючи сесію зі стану користувача.
            ///
            /// Викликається після того, як похідний план змінив
            /// статуси та вилучив відповідні обмеження.
            ///
            /// (Applies the mutation shared by all plans by removing
            /// the session from the user state.
            ///
            /// Called after the derived plan has changed the statuses
            /// and removed the corresponding restrictions.)
            /// </summary>
            /// <param name="user">Користувач, стан якого змінюється.</param>
            /// <param name="now">Зафіксований час виконання операції.</param>
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
            /// Викликається після запису специфічної для сценарію
            /// доменної події та змін обмежень.
            ///
            /// (Records the session deletion aggregate change
            /// shared by all plans.
            ///
            /// Called after the scenario-specific domain event
            /// and restriction changes have been recorded.)
            /// </summary>
            /// <param name="user">Користувач, для якого записується зміна.</param>
            /// <param name="now">Час виникнення зміни.</param>
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
        /// План повного відкликання активної сесії обмежень.
        ///
        /// Переводить усі обмеження сесії у стан відкликаних,
        /// вилучає їх і саму сесію зі стану користувача,
        /// записує подію відкликання сесії та відповідні
        /// зміни агрегату.
        ///
        /// (Plan for completely revoking an active restriction session.
        ///
        /// Marks all session restrictions as revoked, removes them
        /// and the session from the user state, and records the session
        /// revocation event and corresponding aggregate changes.)
        /// </summary>
        private sealed class RevokeActiveRestrictionSessionPlan : RemovalSessionPlan
        {
            public IReadOnlyCollection<UserRestriction> RestrictionsToRevoke { get; }

            public RevokeActiveRestrictionSessionPlan(
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

                base.Apply(
                    user,
                    now
                );
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

                base.RecordEventsAndChanges(
                    user,
                    now
                );
            }
        }

        /// <summary>
        /// План завершення сесії, строк дії якої вже сплив.
        ///
        /// Переводить усі обмеження сесії у стан завершених,
        /// вилучає їх і саму сесію зі стану користувача,
        /// записує подію завершення сесії та відповідні
        /// зміни агрегату.
        ///
        /// (Plan for completing a restriction session that has already expired.
        ///
        /// Marks all session restrictions as completed, removes them
        /// and the session from the user state, and records the session
        /// completion event and corresponding aggregate changes.)
        /// </summary>
        private sealed class CompleteExpiredRestrictionSessionPlan : RemovalSessionPlan
        {
            public IReadOnlyCollection<UserRestriction> RestrictionsToComplete { get; }

            public CompleteExpiredRestrictionSessionPlan(
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

                base.Apply(
                    user,
                    now
                );
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

                base.RecordEventsAndChanges(
                    user,
                    now
                );
            }
        }
        #endregion

        #region Session Revocation Orchestration
        /// <summary>
        /// Координує повне припинення знайденої сесії.
        ///
        /// Спочатку готує та перевіряє план операції.
        /// Якщо підготовка завершилася помилкою, повертає її
        /// без зміни стану агрегату. Після успішної підготовки
        /// застосовує план і записує події та зміни агрегату.
        ///
        /// (Coordinates the complete termination of an existing session.
        ///
        /// First prepares and validates the operation plan.
        /// If preparation fails, returns the error without mutating
        /// the aggregate. After successful preparation, applies the plan
        /// and records its events and aggregate changes.)
        /// </summary>
        /// <param name="session">Сесія, яку необхідно припинити.</param>
        /// <param name="restrictions">Усі активні обмеження цієї сесії.</param>
        /// <param name="now">Зафіксований час виконання операції.</param>
        /// <returns>
        /// Успішний результат або помилка, отримана
        /// під час підготовки плану.
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

            return Result.Success();
        }
        #endregion

        #region Session Revocation Plan Preparation
        /// <summary>
        /// Перевіряє відповідність сесії її активним обмеженням
        /// і готує план повного припинення сесії без зміни агрегату.
        ///
        /// Для вже простроченої сесії готує план завершення
        /// обмежень. Для активної сесії, яка стане порожньою,
        /// готує план їх відкликання.
        ///
        /// Оскільки передається повний склад сесії, результати,
        /// за яких вона залишається непорожньою, вважаються
        /// порушенням стану агрегату.
        ///
        /// (Verifies that the session exactly matches its active restrictions
        /// and prepares a complete session-termination plan without mutating
        /// the aggregate.
        ///
        /// Prepares a restriction-completion plan for an already expired
        /// session and a revocation plan for an active session that becomes empty.
        ///
        /// Because the complete session contents are supplied, outcomes in which
        /// the session remains non-empty are treated as an aggregate-state violation.)
        /// </summary>
        /// <param name="session">Сесія, для якої готується план.</param>
        /// <param name="restrictions">Повний склад активних обмежень сесії.</param>
        /// <param name="now">Зафіксований час, відносно якого визначається стан сесії.</param>
        /// <returns>
        /// Успішний результат із підготовленим планом або
        /// помилка перевірки відкликання обмежень.
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо склад сесії не відповідає обмеженням
        /// агрегату або отримано неможливий результат вилучення.
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає для непідтримуваного значення результату вилучення.
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
                    OperationType.Update
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
                        OperationType.Update
                    ),

                _ => throw DomainDataInconsistencyException.UnsupportedDiscriminator<User>(
                    nameof(removalEffect),
                    removalEffect,
                    OperationType.Update
                )
            };
        }

        /// <summary>
        /// Перевіряє можливість завершення всіх обмежень
        /// простроченої сесії та створює відповідний план.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Validates that all restrictions of an expired session
        /// can be completed and creates the corresponding plan.
        ///
        /// The method does not mutate the aggregate.)
        /// </summary>
        /// <param name="session">Прострочена сесія.</param>
        /// <param name="restrictions">Обмеження, які необхідно перевести у стан завершених.</param>
        /// <param name="now">Зафіксований час завершення.</param>
        /// <returns>Успішний результат із підготовленим планом.</returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо хоча б одне обмеження не може
        /// перейти у стан завершеного.
        /// </exception>
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
        /// активної сесії та створює відповідний план.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Validates that all restrictions of an active session
        /// can be revoked and creates the corresponding plan.
        ///
        /// The method does not mutate the aggregate.)
        /// </summary>
        /// <param name="session">Активна сесія, яку необхідно відкликати.</param>
        /// <param name="restrictions">Обмеження, які необхідно відкликати.</param>
        /// <param name="now">Зафіксований час відкликання.</param>
        /// <returns>
        /// Успішний результат із підготовленим планом або
        /// перша помилка перевірки відкликання.
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
