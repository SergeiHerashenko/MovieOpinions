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

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        #region Add Restrictions
        /// <summary>
        /// Додає користувачу одне або кілька активних обмежень.
        ///
        /// Перевіряє вхідні дані та часову послідовність операції,
        /// створює обмеження й готує плани створення, оновлення
        /// або заміни відповідних сесій. Сесію із завершеним строком
        /// замінює новою після завершення її поточних обмежень.
        ///
        /// Після застосування планів приєднує нові обмеження
        /// та записує доменні події й зміни агрегату.
        /// Якщо додано хоча б одне обмеження типу Ban,
        /// завершує всі поточні refresh-токени користувача.
        /// Наприкінці успішної операції оновлює UpdatedAt.
        ///
        /// (Adds one or more active restrictions to the user.
        ///
        /// Validates input and operation chronology, creates restrictions,
        /// and prepares plans for creating, updating, or replacing
        /// the corresponding sessions. Replaces an expired session
        /// after completing its current restrictions.
        ///
        /// After applying the plans, attaches the new restrictions
        /// and records domain events and aggregate changes.
        /// If at least one Ban restriction is added, terminates
        /// all current user refresh tokens. Updates UpdatedAt
        /// at the end of a successful operation.)
        /// </summary>
        /// <remarks>
        /// Додавання лише мутів не завершує refresh-токени.
        /// Під час завершення токенів прострочені переходять у Expired,
        /// а решта активних — у Revoked.
        ///
        /// Підготовка обмежень і планів виконується до їх застосування.
        /// Метод не виконує автоматичного відкату змін у пам’яті,
        /// якщо під час застосування виникне виняток.
        ///
        /// (Adding only mute restrictions does not terminate refresh tokens.
        /// During termination, elapsed tokens become Expired
        /// and the remaining active tokens become Revoked.
        ///
        /// Restrictions and plans are prepared before being applied.
        /// The method does not automatically revert in-memory changes
        /// if an exception occurs during application.)
        /// </remarks>
        /// <param name="restrictionDataCollection">
        /// Дані обмежень, які необхідно створити та додати користувачу.
        ///
        /// (Data for the restrictions to create and add to the user.)
        /// </param>
        /// <param name="now">
        /// Фіксований час усієї операції. Не може передувати часу
        /// створення, останнього оновлення чи останнього успішного входу
        /// користувача за наявності відповідних часових позначок.
        ///
        /// (The fixed timestamp for the entire operation. It cannot precede
        /// the user's creation, last update, or last successful sign-in
        /// timestamp when the corresponding values are present.)
        /// </param>
        /// <returns>
        /// Успіх після додавання обмежень та виконання пов’язаних змін;
        /// інакше — доменна помилка порожньої колекції.
        ///
        /// (Success after adding restrictions and applying related changes;
        /// otherwise, a domain error indicating an empty collection.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язкові дані відсутні,
        /// тип обмеження не підтримується або порушено часові передумови.
        ///
        /// (Thrown when required data is missing, a restriction type
        /// is unsupported, or timestamp preconditions are violated.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо колекція містить null-елементи
        /// або порушено внутрішні передумови застосування змін.
        ///
        /// (Thrown when the collection contains null entries
        /// or internal preconditions for applying changes are violated.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження та сесії утворюють неузгоджений стан.
        ///
        /// (Thrown when restrictions and sessions form an inconsistent state.)
        /// </exception>
        /// <exception cref="OverflowException">
        /// Виникає, якщо сумарна тривалість обмежень виходить за межі Int32.
        ///
        /// (Thrown when the total restriction duration exceeds the Int32 range.)
        /// </exception>
        public Result AddRestrictions(
            IEnumerable<RestrictionData> restrictionDataCollection,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (restrictionDataCollection, nameof(restrictionDataCollection))
            );

            EnsureValidOperationTimestamp(now);

            var restrictionData = restrictionDataCollection.ToList();

            if (restrictionData.Count == 0)
            {
                return Result.Failure(UserErrors.EmptyRestrictionList<User>());
            }

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

            if (preparedGroups.Any(
                group => group.RestrictionType == RestrictionType.Ban))
            {
                TerminateAllRefreshToken(now);
            }

            UpdatedAt = now;

            return Result.Success();
        }
        #endregion

        #region Restriction Preparation
        /// <summary>
        /// Містить підготовлену групу новостворених обмежень
        /// одного типу та їх описи для доменних подій.
        ///
        /// Склад колекцій копіюється під час створення групи.
        /// Підготовка групи не приєднує обмеження до агрегату.
        ///
        /// (Contains a prepared group of newly created restrictions
        /// of the same type and their descriptions for domain events.
        ///
        /// Collection membership is copied when the group is created.
        /// Preparing the group does not attach restrictions to the aggregate.)
        /// </summary>
        private sealed class PreparedRestrictionGroup
        {
            /// <summary>
            /// Тип обмежень, об’єднаних у підготовлену групу.
            ///
            /// (Type of the restrictions in the prepared group.)
            /// </summary>
            public RestrictionType RestrictionType { get; }

            /// <summary>
            /// Новостворені обмеження підготовленої групи.
            ///
            /// (Newly created restrictions in the prepared group.)
            /// </summary>
            public IReadOnlyCollection<UserRestriction> CreatedRestrictions { get; }

            /// <summary>
            /// Правила та причини обмежень, підготовлені
            /// на основі вхідних даних для доменних подій.
            ///
            /// (Restriction rules and reasons prepared
            /// from the input data for domain events.)
            /// </summary>
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
        /// Групує вхідні дані за типом обмеження та створює
        /// відповідні обмеження й описи для доменних подій.
        ///
        /// Нові обмеження створюються окремо від агрегату.
        /// Метод не змінює поточний стан користувача.
        ///
        /// (Groups input data by restriction type and creates
        /// the corresponding restrictions and descriptions for domain events.
        ///
        /// New restrictions are created separately from the aggregate.
        /// The method does not modify the user's current state.)
        /// </summary>
        /// <param name="restrictionData">
        /// Матеріалізовані дані обмежень без null-елементів.
        ///
        /// (Materialized restriction data containing no null elements.)
        /// </param>
        /// <param name="now">
        /// Час створення нових обмежень.
        ///
        /// (Creation time of the new restrictions.)
        /// </param>
        /// <returns>
        /// Підготовлені групи обмежень з описами для доменних подій.
        ///
        /// (Prepared restriction groups with descriptions for domain events.)
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
        /// Створює обмеження для поточного користувача
        /// на основі переданих даних.
        ///
        /// Не приєднує створені обмеження до агрегату,
        /// що дозволяє підготувати їх до початку зміни стану користувача.
        ///
        /// (Creates restrictions for the current user
        /// from the supplied data.
        ///
        /// Does not attach the created restrictions to the aggregate,
        /// allowing them to be prepared before modifying user state.)
        /// </summary>
        /// <param name="restrictionDataCollection">
        /// Дані обмежень, які необхідно створити.
        ///
        /// (Data for the restrictions to create.)
        /// </param>
        /// <param name="now">
        /// Час створення всіх обмежень колекції.
        ///
        /// (Creation time of all restrictions in the collection.)
        /// </param>
        /// <returns>
        /// Колекція новостворених обмежень, ще не приєднаних до агрегату.
        ///
        /// (Collection of newly created restrictions not yet attached to the aggregate.)
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
        /// Представляє виконуваний план додавання підготовленої
        /// групи обмежень у межах відповідної сесії.
        ///
        /// Визначає окремі етапи застосування доменного стану
        /// та запису доменних подій і змін агрегату.
        /// Створення плану не змінює стан агрегату.
        ///
        /// (Represents an executable plan for adding a prepared
        /// restriction group within its corresponding session.
        ///
        /// Defines separate stages for applying domain state
        /// and recording domain events and aggregate changes.
        /// Creating the plan does not modify aggregate state.)
        /// </summary>
        private abstract class RestrictionAdditionPlan
        {
            /// <summary>
            /// Підготовлена група нових обмежень,
            /// для якої виконується план.
            ///
            /// (Prepared group of new restrictions
            /// for which the plan is executed.)
            /// </summary>
            public PreparedRestrictionGroup Group { get; }

            public RestrictionAdditionPlan(PreparedRestrictionGroup group)
            {
                Group = group;
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
        /// Представляє план додавання підготовленої нової сесії
        /// для групи обмежень, що не має відповідної наявної сесії.
        ///
        /// (Represents a plan for adding a prepared new session
        /// for a restriction group without a corresponding existing session.)
        /// </summary>
        private sealed class CreateRestrictionSessionPlan : RestrictionAdditionPlan
        {
            /// <summary>
            /// Підготовлена нова сесія, яку план додає до агрегату.
            ///
            /// (Prepared new session that the plan adds to the aggregate.)
            /// </summary>
            public UserRestrictionSession NewSession { get; }

            public CreateRestrictionSessionPlan(
                PreparedRestrictionGroup group,
                UserRestrictionSession newSession)
                : base(group)
            {
                NewSession = newSession;
            }

            /// <summary>
            /// Додає підготовлену нову сесію
            /// до активної колекції сесій агрегату.
            ///
            /// (Adds the prepared new session
            /// to the aggregate's active session collection.)
            /// </summary>
            /// <param name="user">
            /// Користувач, до якого додається сесія.
            ///
            /// (User to which the session is added.)
            /// </param>
            /// <param name="now">
            /// Час операції, переданий спільним контрактом плану.
            /// У цьому варіанті плану параметр не використовується.
            ///
            /// (Operation time passed through the shared plan contract.
            /// This implementation does not use the parameter.)
            /// </param>
            public override void Apply(
                User user,
                DateTimeOffset now)
            {
                user.AddRestrictionSession(NewSession);
            }

            /// <summary>
            /// Записує доменну подію створення нової сесії
            /// та відповідну зміну агрегату.
            ///
            /// Подія містить описи обмежень, тип сесії
            /// та її загальну тривалість.
            ///
            /// (Records a domain event for the new session's creation
            /// and the corresponding aggregate change.
            ///
            /// The event includes restriction descriptions, session type,
            /// and total session duration.)
            /// </summary>
            /// <param name="user">
            /// Користувач, до якого додаються подія та зміна агрегату.
            ///
            /// (User to which the event and aggregate change are added.)
            /// </param>
            /// <param name="now">
            /// Час операції, що передається до події та запису зміни.
            ///
            /// (Operation time passed to the event and change record.)
            /// </param>
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
            /// <summary>
            /// Наявна сесія, яку план оновлює додаванням нових обмежень.
            ///
            /// (Existing session that the plan updates by adding new restrictions.)
            /// </summary>
            public UserRestrictionSession ExistingSession { get; }

            public UpdateRestrictionSessionPlan(
                PreparedRestrictionGroup group,
                UserRestrictionSession existingSession)
                : base(group)
            {
                ExistingSession = existingSession;
            }

            /// <summary>
            /// Додає обмеження підготовленої групи
            /// до наявної сесії.
            ///
            /// (Adds the prepared group's restrictions
            /// to the existing session.)
            /// </summary>
            /// <param name="user">
            /// Агрегат користувача, для якого підготовлено план.
            /// Ця реалізація використовує збережене посилання на сесію
            /// та безпосередньо не звертається до параметра.
            ///
            /// (User aggregate for which the plan was prepared.
            /// This implementation uses the stored session reference
            /// and does not directly access the parameter.)
            /// </param>
            /// <param name="now">
            /// Час додавання нових обмежень до сесії.
            ///
            /// (Time used for adding new restrictions to the session.)
            /// </param>
            public override void Apply(
                User user,
                DateTimeOffset now)
            {
                ExistingSession.AddRestrictions(
                    Group.CreatedRestrictions,
                    now
                );
            }

            /// <summary>
            /// Записує доменну подію додавання обмежень до сесії
            /// та зміну агрегату про її оновлення.
            ///
            /// Подія містить описи доданих обмежень
            /// і загальну тривалість уже оновленої сесії.
            ///
            /// (Records a domain event for adding restrictions to the session
            /// and an aggregate change for the session's update.
            ///
            /// The event includes descriptions of the added restrictions
            /// and the total duration of the updated session.)
            /// </summary>
            /// <param name="user">
            /// Користувач, до якого додаються подія та зміна агрегату.
            ///
            /// (User to which the event and aggregate change are added.)
            /// </param>
            /// <param name="now">
            /// Час операції, що передається до події та запису зміни.
            ///
            /// (Operation time passed to the event and change record.)
            /// </param>
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
        /// Представляє план заміни сесії із завершеним строком новою.
        ///
        /// Під час застосування завершує обмеження старої сесії,
        /// видаляє її з активної колекції та додає підготовлену нову сесію.
        ///
        /// Фіксує склад обмежень, які необхідно завершити,
        /// і окремо записує доменні події та зміни агрегату.
        ///
        /// (Represents a plan for replacing a session whose lifetime
        /// has ended with a new one.
        ///
        /// When applied, completes the old session's restrictions,
        /// removes the session from the active collection,
        /// and adds the prepared new session.
        ///
        /// Captures the restrictions to complete and separately records
        /// domain events and aggregate changes.)
        /// </summary>
        private sealed class ReplaceExpiredRestrictionSessionPlan : RestrictionAdditionPlan
        {
            /// <summary>
            /// Сесія із завершеним строком, яку необхідно замінити.
            ///
            /// (Session whose lifetime has ended and which must be replaced.)
            /// </summary>
            public UserRestrictionSession ExpiredSession { get; }

            /// <summary>
            /// Підготовлена нова сесія, що замінює попередню.
            ///
            /// (Prepared new session that replaces the previous one.)
            /// </summary>
            public UserRestrictionSession ReplacementSession { get; }

            /// <summary>
            /// Зафіксована колекція обмежень старої сесії,
            /// які необхідно завершити під час застосування плану.
            ///
            /// (Captured collection of the old session's restrictions
            /// to complete when applying the plan.)
            /// </summary>
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

            /// <summary>
            /// Завершує обмеження старої сесії та видаляє їх
            /// з активної колекції обмежень агрегату.
            ///
            /// Після цього видаляє стару сесію
            /// та додає підготовлену сесію заміни.
            ///
            /// (Completes the old session's restrictions and removes them
            /// from the aggregate's active restriction collection.
            ///
            /// Then removes the old session
            /// and adds the prepared replacement session.)
            /// </summary>
            /// <param name="user">
            /// Користувач, обмеження та сесія якого замінюються.
            ///
            /// (User whose restrictions and session are replaced.)
            /// </param>
            /// <param name="now">
            /// Час, яким фіксується завершення попередніх обмежень.
            ///
            /// (Time recorded for completing the previous restrictions.)
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

                user.RemoveRestrictionSession(ExpiredSession);
                user.AddRestrictionSession(ReplacementSession);
            }

            /// <summary>
            /// Записує наслідки вже застосованої заміни сесії.
            ///
            /// Створює доменні події завершення старої сесії
            /// та створення нової. Записує оновлення завершених обмежень,
            /// видалення старої сесії та створення сесії заміни.
            ///
            /// Не виконує повторних переходів доменного стану.
            ///
            /// (Records the consequences of an already applied session replacement.
            ///
            /// Creates domain events for completing the old session
            /// and creating the new one. Records updates to completed restrictions,
            /// deletion of the old session, and creation of the replacement session.
            ///
            /// Does not repeat domain-state transitions.)
            /// </summary>
            /// <param name="user">
            /// Користувач, до якого додаються події та зміни агрегату.
            ///
            /// (User to which events and aggregate changes are added.)
            /// </param>
            /// <param name="now">
            /// Час операції, що передається до подій і записів змін.
            ///
            /// (Operation time passed to events and change records.)
            /// </param>
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
        /// Готує виконуваний план для кожної підготовленої групи обмежень.
        ///
        /// Якщо відповідна сесія відсутня, створює нову сесію
        /// та готує план її додавання.
        ///
        /// Якщо строк наявної сесії завершився, перевіряє точний збіг
        /// її ідентифікаторів з активними обмеженнями агрегату її типу,
        /// перевіряє можливість завершення цих обмежень
        /// і готує нову сесію та план заміни.
        ///
        /// Якщо строк сесії ще не завершився, перевіряє можливість
        /// додавання нових обмежень і готує план її оновлення.
        ///
        /// Метод не змінює поточний стан агрегату.
        ///
        /// (Prepares an executable plan for each prepared restriction group.
        ///
        /// If the corresponding session is absent, creates a new session
        /// and prepares a plan to add it.
        ///
        /// If an existing session's lifetime has ended, verifies that its
        /// restriction identifiers exactly match the aggregate's active
        /// restrictions of its type, validates their completion,
        /// and prepares a new session and replacement plan.
        ///
        /// If the session's lifetime has not ended, validates that new
        /// restrictions can be added and prepares an update plan.
        ///
        /// The method does not modify the current aggregate state.)
        /// </summary>
        /// <param name="preparedGroups">
        /// Підготовлені групи новостворених обмежень.
        ///
        /// (Prepared groups of newly created restrictions.)
        /// </param>
        /// <param name="now">
        /// Фіксований час перевірок і створення нових сесій.
        ///
        /// (Fixed time used for validation and new session creation.)
        /// </param>
        /// <returns>
        /// Підготовлені та перевірені плани додавання, оновлення або заміни сесій.
        ///
        /// (Prepared and validated plans for adding, updating, or replacing sessions.)
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо набір ідентифікаторів у простроченій сесії
        /// не збігається з активними обмеженнями агрегату її типу.
        ///
        /// (Thrown when an expired session's restriction identifier set
        /// does not match the aggregate's active restrictions of its type.)
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
        /// Послідовно застосовує всі підготовлені
        /// плани додавання обмежень.
        ///
        /// Делегує кожному плану додавання, оновлення
        /// або заміну відповідної сесії.
        ///
        /// Повинен викликатися лише після успішної
        /// підготовки та перевірки всіх планів операції.
        ///
        /// (Sequentially applies all prepared restriction-addition plans.
        ///
        /// Delegates adding, updating, or replacing
        /// the corresponding session to each plan.
        ///
        /// Must be called only after all operation plans
        /// have been successfully prepared and validated.)
        /// </summary>
        /// <param name="additionPlans">
        /// Підготовлені та перевірені плани додавання обмежень.
        ///
        /// (Prepared and validated restriction-addition plans.)
        /// </param>
        /// <param name="now">
        /// Фіксований час застосування всіх планів.
        ///
        /// (Fixed time used for applying all plans.)
        /// </param>
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
        /// Записує зміну агрегату про створення
        /// для кожного переданого обмеження.
        ///
        /// Не змінює доменний стан самих обмежень.
        ///
        /// (Records an aggregate creation change
        /// for each supplied restriction.
        ///
        /// Does not modify the restrictions' domain state.)
        /// </summary>
        /// <param name="restrictions">
        /// Новостворені обмеження, для яких необхідно записати створення.
        ///
        /// (Newly created restrictions whose creation must be recorded.)
        /// </param>
        /// <param name="now">
        /// Час операції, що передається до записів змін агрегату.
        ///
        /// (Operation time passed to the aggregate-change records.)
        /// </param>
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
        /// Записує доменні події та зміни агрегату
        /// для всіх уже застосованих планів додавання обмежень.
        ///
        /// Делегує кожному плану запис наслідків
        /// додавання, оновлення або заміни відповідної сесії.
        ///
        /// Не виконує повторних переходів доменного стану.
        ///
        /// (Records domain events and aggregate changes
        /// for all restriction-addition plans that have already been applied.
        ///
        /// Delegates recording the consequences of adding, updating,
        /// or replacing the corresponding session to each plan.
        ///
        /// Does not repeat domain-state transitions.)
        /// </summary>
        /// <param name="additionPlans">
        /// Застосовані плани додавання обмежень.
        ///
        /// (Applied restriction-addition plans.)
        /// </param>
        /// <param name="now">
        /// Час операції, що передається до подій і записів змін.
        ///
        /// (Operation time passed to events and change records.)
        /// </param>
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
        /// Додає попередньо створену сесію
        /// до активної колекції сесій агрегату.
        ///
        /// Повинен викликатися лише для сесії,
        /// створення якої було попередньо перевірено.
        ///
        /// (Adds a previously created session
        /// to the aggregate's active session collection.
        ///
        /// Must be called only for a session
        /// whose creation has already been validated.)
        /// </summary>
        /// <param name="session">
        /// Підготовлена сесія, яку необхідно додати до агрегату.
        ///
        /// (Prepared session to add to the aggregate.)
        /// </param>
        private void AddRestrictionSession(UserRestrictionSession session)
        {
            _restrictionSessions.Add(session);
        }
        #endregion
    }
}
