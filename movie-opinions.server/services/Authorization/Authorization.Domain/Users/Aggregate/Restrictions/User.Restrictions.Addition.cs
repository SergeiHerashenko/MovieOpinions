using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Restriction;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.DomainEvents.SessionRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.Errors;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        #region Add Restrictions
        /// <summary>
        /// Додає користувачу одне або кілька активних обмежень.
        ///
        /// Перед зміною стану агрегату створює обмеження, групує
        /// їх за типом і готує перевірені плани створення,
        /// оновлення або заміни відповідних сесій.
        ///
        /// Якщо наявна сесія вже завершилася, її активні обмеження
        /// переводяться у завершений стан, а сама сесія замінюється новою.
        ///
        /// Після успішного застосування всіх планів записує
        /// доменні події та зміни агрегату.
        ///
        /// (Adds one or more active restrictions to the user.
        ///
        /// Before modifying the aggregate, creates the restrictions,
        /// groups them by type, and prepares validated plans for creating,
        /// updating, or replacing the corresponding sessions.
        ///
        /// If an existing session has expired, its active restrictions
        /// are completed and the session is replaced with a new one.
        ///
        /// After successfully applying all plans, records the resulting
        /// domain events and aggregate changes.)
        /// </summary>
        /// <param name="restrictionDataCollection">Дані обмежень, які необхідно додати.</param>
        /// <param name="now">Фіксований час виконання операції.</param>
        /// <returns>
        /// Успіх або очікувана помилка, якщо не передано
        /// жодного обмеження.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо колекція відсутня, містить null-елементи
        /// або переданий час є некоректним.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо стан сесії та активних обмежень
        /// агрегату є неузгодженим.
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо отримано непідтримуваний тип плану.
        /// </exception>
        public Result AddRestrictions(
            IEnumerable<RestrictionData> restrictionDataCollection,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (restrictionDataCollection, nameof(restrictionDataCollection))
            );

            DomainGuard.AgainstEarlierThan<User>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            var restrictionData = restrictionDataCollection.ToList();

            if (restrictionData.Count == 0)
                return Result.Failure(UserErrors.EmptyRestrictionList<User>());

            if (restrictionData.Any(x => x is null))
            {
                throw DomainInvalidOperationException.PreconditionFailed<User>(
                    nameof(AddRestrictions),
                    "All restriction data items must be non-null.",
                    OperationType.Update,
                    context: new Dictionary<string, object>
                    {
                        ["RestrictionDataCount"] = restrictionData.Count,
                        ["NullItemCount"] = restrictionData.Count(x => x is null)
                    }
                );
            }

            var preparedGroups = PrepareRestrictionGroups(
                restrictionData,
                now
            );

            var sessionPlans = PrepareRestrictionSessionPlans(
                preparedGroups,
                now
            );

            var createdRestrictions = preparedGroups
                .SelectMany(x => x.CreatedRestrictions)
                .ToArray();

            ApplyRestrictionAdditionPlans(
                sessionPlans,
                now
            );

            _restrictions.AddRange(createdRestrictions);

            RecordRestrictionAdditionEventsAndChanges(
                sessionPlans,
                now
            );

            AddRestrictionCreatedChanges(
                createdRestrictions,
                now
            );

            return Result.Success();
        }
        #endregion

        #region Restriction Preparation
        /// <summary>
        /// Містить підготовлену групу новостворених обмежень
        /// одного типу та їх описи для майбутніх доменних подій.
        ///
        /// Склад колекцій фіксується під час створення групи.
        /// Обмеження ще не приєднані до агрегату.
        ///
        /// (Contains a prepared group of newly created restrictions
        /// of the same type together with their descriptions for
        /// future domain events.
        ///
        /// Collection membership is captured when the group is created.
        /// The restrictions have not yet been attached to the aggregate.)
        /// </summary>
        private sealed class PreparedRestrictionGroup
        {
            public RestrictionType RestrictionType { get; }

            public IReadOnlyCollection<UserRestriction> CreatedRestrictions { get; }

            public IReadOnlyCollection<(RestrictionRule Rule, string? Reason)> Descriptions { get; }

            public PreparedRestrictionGroup(
                RestrictionType restrictionType,
                IReadOnlyCollection<UserRestriction> createdRestrictions,
                IReadOnlyCollection<(RestrictionRule Rule, string? Reason)> descriptions)
            {
                RestrictionType = restrictionType;
                CreatedRestrictions = createdRestrictions.ToArray();
                Descriptions = descriptions.ToArray();
            }
        }

        /// <summary>
        /// Групує перевірені вхідні дані за типом і створює
        /// відокремлені від агрегату обмеження разом з описами
        /// для майбутніх доменних подій.
        ///
        /// Метод не змінює поточний стан агрегату.
        ///
        /// (Groups validated input data by restriction type and creates
        /// restrictions detached from the aggregate together with
        /// descriptions for future domain events.
        ///
        /// The method does not modify the current aggregate state.)
        /// </summary>
        /// <param name="restrictionData">Матеріалізовані та перевірені дані обмежень.</param>
        /// <param name="now">Час створення обмежень.</param>
        /// <returns>
        /// Підготовлені групи новостворених обмежень.
        /// </returns>
        private IReadOnlyCollection<PreparedRestrictionGroup> PrepareRestrictionGroups(
            IReadOnlyCollection<RestrictionData> restrictionData,
            DateTimeOffset now)
        {
            var preparedGroups = new List<PreparedRestrictionGroup>();

            foreach (var dataGroup in restrictionData.GroupBy(x => x.RestrictionType))
            {
                var groupData = dataGroup.ToArray();

                var createdRestrictions = CreateRestrictions(
                    groupData,
                    now
                );

                var descriptions = groupData
                    .Select(x => (
                        Rule: x.RestrictionRule,
                        Reason: x.Reason
                    ))
                    .ToArray();

                preparedGroups.Add(new PreparedRestrictionGroup(
                    dataGroup.Key,
                    createdRestrictions,
                    descriptions)
                );
            }

            return preparedGroups.AsReadOnly();
        }

        /// <summary>
        /// Створює обмеження з переданих даних,
        /// не приєднуючи їх до агрегату.
        ///
        /// Це дозволяє створити й перевірити всі обмеження
        /// до початку зміни стану користувача.
        ///
        /// (Creates restrictions from the supplied data without
        /// attaching them to the aggregate.
        ///
        /// This allows every restriction to be created and validated
        /// before the user state begins to change.)
        /// </summary>
        /// <param name="restrictionDataCollection">Дані обмежень для створення.</param>
        /// <param name="now">Час створення обмежень.</param>
        /// <returns>
        /// Колекція новостворених відокремлених обмежень.
        /// </returns>
        private List<UserRestriction> CreateRestrictions(
            IEnumerable<RestrictionData> restrictionDataCollection,
            DateTimeOffset now)
        {
            var restrictions = new List<UserRestriction>();

            foreach (var data in restrictionDataCollection)
            {
                var restriction = UserRestriction.Create(
                    Id,
                    data.RestrictionType,
                    data.RestrictionRule,
                    data.ImposedBy,
                    data.Reason,
                    now
                );

                restrictions.Add(restriction);
            }

            return restrictions;
        }
        #endregion

        #region Session Planning
        /// <summary>
        /// Представляє попередньо перевірений виконуваний план
        /// додавання обмежень у межах однієї сесії.
        ///
        /// План окремо визначає мутацію доменного стану та запис
        /// подій і змін агрегату. Його створення не змінює агрегат.
        ///
        /// (Represents a pre-validated executable plan for adding
        /// restrictions within a single session.
        ///
        /// The plan separately defines domain-state mutation and the
        /// recording of events and aggregate changes. Creating the plan
        /// does not modify the aggregate.)
        /// </summary>
        private abstract class RestrictionAdditionPlan
        {
            public PreparedRestrictionGroup Group { get; }

            public RestrictionAdditionPlan(PreparedRestrictionGroup group)
            {
                Group = group;
            }

            /// <summary>
            /// Застосовує попередньо перевірені переходи стану,
            /// визначені цим планом.
            ///
            /// Метод повинен викликатися лише після успішної
            /// підготовки всіх планів операції.
            ///
            /// (Applies the pre-validated state transitions represented
            /// by this plan.
            ///
            /// The method must be called only after all operation plans
            /// have been successfully prepared.)
            /// </summary>
            /// <param name="user">Агрегат, до якого застосовується план.</param>
            /// <param name="now">Фіксований час виконання операції.</param>
            public abstract void Apply(
                User user,
                DateTimeOffset now);

            /// <summary>
            /// Записує доменні події та зміни агрегату, які відповідають
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
        /// Представляє план створення нової сесії для групи
        /// обмежень, яка не має відповідної активної сесії.
        ///
        /// (Represents a plan for creating a new session for a group
        /// of restrictions that has no corresponding active session.)
        /// </summary>
        private sealed class CreateRestrictionSessionPlan : RestrictionAdditionPlan
        {
            public UserRestrictionSession NewSession { get; }

            public CreateRestrictionSessionPlan(
                PreparedRestrictionGroup group,
                UserRestrictionSession newSession)
                : base(group)
            {
                NewSession = newSession;
            }

            public override void Apply(
                User user,
                DateTimeOffset now)
            {
                user.AddRestrictionSession(NewSession);
            }

            public override void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddDomainEvent(new UserRestrictionSessionCreatedEvent(
                    NewSession.Id,
                    user.Login,
                    Group.Descriptions,
                    NewSession.RestrictionType,
                    NewSession.TotalBlockedMinutes,
                    now)
                );

                user.AddAggregateChange(new UserRestrictionSessionCreated(
                    NewSession,
                    now)
                );
            }
        }

        /// <summary>
        /// Представляє план додавання нових обмежень
        /// до наявної активної сесії.
        ///
        /// (Represents a plan for adding new restrictions
        /// to an existing active session.)
        /// </summary>
        private sealed class UpdateRestrictionSessionPlan : RestrictionAdditionPlan
        {
            public UserRestrictionSession ExistingSession { get; }

            public UpdateRestrictionSessionPlan(
                PreparedRestrictionGroup group,
                UserRestrictionSession existingSession)
                : base(group)
            {
                ExistingSession = existingSession;
            }

            public override void Apply(
                User user,
                DateTimeOffset now)
            {
                ExistingSession.AddRestrictions(
                    Group.CreatedRestrictions,
                    now
                );
            }

            public override void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddDomainEvent(new UserRestrictionsAddedToSessionEvent(
                    ExistingSession.Id,
                    user.Login,
                    Group.Descriptions,
                    ExistingSession.RestrictionType,
                    ExistingSession.TotalBlockedMinutes,
                    now)
                );

                user.AddAggregateChange(new UserRestrictionSessionUpdated(
                    ExistingSession,
                    now)
                );
            }
        }

        /// <summary>
        /// Представляє план заміни простроченої сесії новою.
        ///
        /// Активні обмеження старої сесії переходять у завершений
        /// стан, стара сесія видаляється, а підготовлена група
        /// обмежень утворює нову сесію.
        ///
        /// План фіксує склад обмежень старої сесії, які необхідно
        /// завершити під час застосування.
        ///
        /// (Represents a plan for replacing an expired session
        /// with a new one.
        ///
        /// Active restrictions from the old session transition to the
        /// completed state, the old session is removed, and the prepared
        /// restriction group forms a new session.
        ///
        /// The plan captures the old session restrictions that must
        /// be completed when the plan is applied.)
        /// </summary>
        private sealed class ReplaceExpiredRestrictionSessionPlan : RestrictionAdditionPlan
        {
            public UserRestrictionSession ExpiredSession { get; }

            public UserRestrictionSession ReplacementSession { get; }

            public IReadOnlyCollection<UserRestriction> RestrictionsToComplete { get; }

            public ReplaceExpiredRestrictionSessionPlan(
                PreparedRestrictionGroup group,
                UserRestrictionSession expiredSession,
                UserRestrictionSession replacementSession,
                IReadOnlyCollection<UserRestriction> restrictionsToComplete)
                : base(group)
            {
                ExpiredSession = expiredSession;
                ReplacementSession = replacementSession;
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

                user.RemoveRestrictionSession(ExpiredSession);
                user.AddRestrictionSession(ReplacementSession);
            }

            public override void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddDomainEvent(new UserRestrictionSessionCompletedEvent(
                    ExpiredSession.Id,
                    user.Login,
                    ExpiredSession.RestrictionType,
                    now)
                );

                user.AddDomainEvent(new UserRestrictionSessionCreatedEvent(
                    ReplacementSession.Id,
                    user.Login,
                    Group.Descriptions,
                    ReplacementSession.RestrictionType,
                    ReplacementSession.TotalBlockedMinutes,
                    now)
                );

                user.AddRestrictionUpdatedChanges(
                    RestrictionsToComplete,
                    now
                );

                user.AddAggregateChange(new UserRestrictionSessionDeleted(
                    ExpiredSession.Id,
                    now)
                );

                user.AddAggregateChange(new UserRestrictionSessionCreated(
                    ReplacementSession,
                    now)
                );
            }
        }

        /// <summary>
        /// Готує виконуваний план для кожної підготовленої
        /// групи обмежень.
        ///
        /// Якщо відповідна сесія відсутня, готує її створення.
        /// Якщо сесія активна, перевіряє можливість її оновлення.
        /// Якщо сесія прострочена, перевіряє точну відповідність
        /// її складу активним обмеженням агрегату, можливість
        /// завершення цих обмежень і готує нову сесію.
        ///
        /// Метод не змінює поточний стан агрегату.
        ///
        /// (Prepares an executable plan for every prepared
        /// restriction group.
        ///
        /// If the corresponding session is absent, prepares its creation.
        /// If the session is active, validates that it can be updated.
        /// If the session is expired, verifies that its contents exactly
        /// match the aggregate’s active restrictions, validates their
        /// completion, and prepares a replacement session.
        ///
        /// The method does not modify the current aggregate state.)
        /// </summary>
        /// <param name="preparedGroups">Підготовлені групи новостворених обмежень.</param>
        /// <param name="now">Фіксований час виконання операції.</param>
        /// <returns>
        /// Повністю перевірені плани майбутніх змін сесій.
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо прострочена сесія не відповідає активним
        /// обмеженням агрегату або обмеження не можуть бути завершені.
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено передумови створення,
        /// оновлення або заміни сесії.
        /// </exception>
        private IReadOnlyCollection<RestrictionAdditionPlan> PrepareRestrictionSessionPlans(
            IReadOnlyCollection<PreparedRestrictionGroup> preparedGroups,
            DateTimeOffset now)
        {
            var plans = new List<RestrictionAdditionPlan>();

            foreach (var preparedGroup in preparedGroups)
            {
                var matchingSession = _restrictionSessions
                    .FirstOrDefault(x => x.RestrictionType == preparedGroup.RestrictionType);

                if (matchingSession is null)
                {
                    var createdSession = UserRestrictionSession.Create(
                        Id,
                        preparedGroup.CreatedRestrictions,
                        now
                    );

                    plans.Add(new CreateRestrictionSessionPlan(
                        preparedGroup,
                        createdSession)
                    );

                    continue;
                }

                if (matchingSession.IsExpiredAt(now))
                {
                    var restrictionsToComplete = _restrictions
                        .Where(x => x.RestrictionType == matchingSession.RestrictionType)
                        .ToArray();

                    if (!matchingSession.ContainsExactly(restrictionsToComplete.Select(x => x.Id)))
                    {
                        throw DomainInvariantViolationException.BrokenState<User>(
                            "The expired restriction session does not exactly match " +
                            "the active aggregate restrictions.",
                            new Dictionary<string, object?>
                            {
                                ["SessionId"] = matchingSession.Id.Value,
                                ["RestrictionType"] = matchingSession.RestrictionType,
                                ["SessionRestrictionIds"] = matchingSession.ActiveRestrictionIds
                                    .Select(x => x.Value)
                                    .ToArray(),
                                ["AggregateRestrictionIds"] = restrictionsToComplete
                                    .Select(x => x.Id.Value)
                                    .ToArray()
                            },
                            OperationType.Update
                        );
                    }

                    foreach (var restriction in restrictionsToComplete)
                    {
                        restriction.ValidateCanComplete(now);
                    }

                    var replacementSession = UserRestrictionSession.Create(
                        Id,
                        preparedGroup.CreatedRestrictions,
                        now
                    );

                    plans.Add(new ReplaceExpiredRestrictionSessionPlan(
                        preparedGroup,
                        matchingSession,
                        replacementSession,
                        restrictionsToComplete)
                    );

                    continue;
                }

                matchingSession.ValidateCanAddRestrictions(
                    preparedGroup.CreatedRestrictions,
                    now
                );

                plans.Add(new UpdateRestrictionSessionPlan(
                    preparedGroup,
                    matchingSession)
                );
            }

            return plans.AsReadOnly();
        }
        #endregion

        #region Addition Plan Application
        /// <summary>
        /// Послідовно застосовує всі попередньо перевірені
        /// плани додавання обмежень.
        ///
        /// Делегує кожному плану створення, оновлення
        /// або заміну відповідної сесії.
        ///
        /// Метод повинен викликатися лише після успішної
        /// підготовки всіх планів.
        ///
        /// (Sequentially applies all previously validated
        /// restriction-addition plans.
        ///
        /// Delegates creation, updating, or replacement of the
        /// corresponding session to each plan.
        ///
        /// The method must be called only after every plan has
        /// been successfully prepared.)
        /// </summary>
        /// <param name="additionPlans">Попередньо перевірені плани додавання обмежень.</param>
        /// <param name="now">Фіксований час виконання операції.</param>
        private void ApplyRestrictionAdditionPlans(
            IReadOnlyCollection<RestrictionAdditionPlan> additionPlans,
            DateTimeOffset now)
        {
            foreach (var plan in additionPlans)
            {
                plan.Apply(
                    this,
                    now
                );
            }
        }
        #endregion

        #region Addition Events And Changes
        /// <summary>
        /// Записує зміну агрегату для кожного
        /// новоствореного обмеження.
        ///
        /// Метод не змінює доменний стан обмежень.
        ///
        /// (Records an aggregate change for every newly
        /// created restriction.
        ///
        /// The method does not modify the domain state
        /// of the restrictions.)
        /// </summary>
        /// <param name="restrictions">Новостворені обмеження, які необхідно зберегти.</param>
        /// <param name="now">Час виникнення змін агрегату.</param>
        private void AddRestrictionCreatedChanges(
            IEnumerable<UserRestriction> restrictions,
            DateTimeOffset now)
        {
            foreach (var restriction in restrictions)
            {
                AddAggregateChange(new UserRestrictionCreated(
                    restriction,
                    now)
                );
            }
        }

        /// <summary>
        /// Записує доменні події та зміни агрегату для всіх
        /// уже застосованих планів додавання обмежень.
        ///
        /// Делегує кожному плану запис наслідків відповідного
        /// сценарію створення, оновлення або заміни сесії.
        ///
        /// Метод не виконує повторних переходів доменного стану.
        ///
        /// (Records domain events and aggregate changes for all
        /// restriction-addition plans that have already been applied.
        ///
        /// Delegates recording of the corresponding session creation,
        /// update, or replacement consequences to each plan.
        ///
        /// The method does not perform domain-state transitions again.)
        /// </summary>
        /// <param name="additionPlans">Застосовані плани додавання обмежень.</param>
        /// <param name="now">Фіксований час виконання операції.</param>
        private void RecordRestrictionAdditionEventsAndChanges(
            IReadOnlyCollection<RestrictionAdditionPlan> additionPlans,
            DateTimeOffset now)
        {
            foreach (var plan in additionPlans)
            {
                plan.RecordEventsAndChanges(
                    this,
                    now
                );
            }
        }
        #endregion

        #region Addition Helpers
        /// <summary>
        /// Додає попередньо створену сесію до активної
        /// колекції сесій агрегату.
        ///
        /// Метод повинен викликатися лише для сесії,
        /// створення якої було попередньо перевірено.
        ///
        /// (Adds a previously created session to the aggregate’s
        /// active session collection.
        ///
        /// The method must be called only for a session whose
        /// creation has already been validated.)
        /// </summary>
        /// <param name="session">Сесія, яку необхідно додати.</param>
        private void AddRestrictionSession(UserRestrictionSession session)
        {
            _restrictionSessions.Add(session);
        }
        #endregion
    }
}
