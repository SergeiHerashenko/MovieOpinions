using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.Enums;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Entities.UsersRestrictionSession
{
    /// <summary>
    /// Представляє активну сесію накопичених обмежень одного типу
    /// для конкретного користувача.
    /// Зберігає унікальні ідентифікатори активних обмежень
    /// та їхню сумарну тривалість.
    ///
    /// (Represents an active session of accumulated restrictions
    /// of one type for a specific user.
    /// Stores unique identifiers of active restrictions
    /// and their total duration.)
    /// </summary>
    public sealed class UserRestrictionSession : Entity<UserRestrictionSessionId>, IUserOwned
    {
        #region Properties
        /// <summary>
        /// Ідентифікатор користувача, якому належить сесія обмежень.
        ///
        /// (Identifier of the user who owns the restriction session.)
        /// </summary>
        public UserId UserId { get; }

        private readonly HashSet<UserRestrictionId> _activeRestrictionIds = new();

        /// <summary>
        /// Унікальні ідентифікатори обмежень, які зараз входять до сесії.
        ///
        /// (Unique identifiers of restrictions currently included in the session.)
        /// </summary>
        public IReadOnlyCollection<UserRestrictionId> ActiveRestrictionIds => _activeRestrictionIds;

        /// <summary>
        /// Тип усіх обмежень, що входять до цієї сесії.
        ///
        /// (Type shared by all restrictions included in this session.)
        /// </summary>
        public RestrictionType RestrictionType { get; }

        /// <summary>
        /// Загальна накопичена тривалість активних обмежень у хвилинах.
        ///
        /// (Total accumulated duration of active restrictions in minutes.)
        /// </summary>
        public int TotalBlockedMinutes { get; private set; }
        #endregion

        #region Constructor
        private UserRestrictionSession(
            UserRestrictionSessionId userRestrictionSessionId,
            UserId userId,
            IReadOnlyCollection<UserRestrictionId> userRestrictionIds,
            RestrictionType restrictionType,
            int totalBlockedMinutes,
            DateTimeOffset createdAt)
            : base(userRestrictionSessionId, createdAt)
        {
            UserId = userId;
            _activeRestrictionIds.UnionWith(userRestrictionIds);
            RestrictionType = restrictionType;
            TotalBlockedMinutes = totalBlockedMinutes;
        }
        #endregion

        #region Creation
        /// <summary>
        /// Створює сесію з початкового непорожнього набору обмежень,
        /// що належать одному користувачу та мають однаковий тип.
        ///
        /// (Creates a session from an initial non-empty collection of restrictions
        /// that belong to the same user and have the same type.)
        /// </summary>
        /// <param name="userId">Ідентифікатор власника сесії.</param>
        /// <param name="userRestrictions">Початковий набір активних обмежень.</param>
        /// <param name="now">Час створення сесії.</param>
        /// <returns>Створена сесія обмежень.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язкові дані відсутні або набір обмежень порожній.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження належать різним користувачам,
        /// мають різні типи або містять дублікати ідентифікаторів.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Виникає, якщо сумарна тривалість обмежень перевищує діапазон Int32.
        /// </exception>
        internal static UserRestrictionSession Create(
            UserId userId,
            IEnumerable<UserRestriction> userRestrictions,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<UserRestrictionSession>(
                OperationType.Create,
                (userId, nameof(userId)),
                (userRestrictions, nameof(userRestrictions))
            );
            
            var restrictions = userRestrictions.ToList();

            if (restrictions.Count == 0)
            {
                throw DomainDataInconsistencyException.Empty<UserRestrictionSession>(
                    nameof(userRestrictions),
                    OperationType.Create
                );
            }

            if (restrictions.Any(id => id is null))
            {
                throw DomainDataInconsistencyException.Empty<UserRestrictionSession>(
                    nameof(userRestrictions),
                    OperationType.Create
                );
            }

            var restrictionForDifferentUser = restrictions
                .FirstOrDefault(r => r.UserId != userId);

            if (restrictionForDifferentUser is not null)
            {
                throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                    "All restrictions used to create a restriction session " +
                    "must belong to the same user.",
                    new Dictionary<string, object?>
                    {
                        ["ExpectedUserId"] = userId.Value,
                        ["ActualUserId"] = restrictionForDifferentUser.UserId.Value,
                        ["RestrictionId"] = restrictionForDifferentUser.Id.Value
                    },
                    OperationType.Create
                );
            }

            var restrictionType = restrictions[0].RestrictionType;

            var restrictionWithDifferentType = restrictions
                .FirstOrDefault(r => r.RestrictionType != restrictionType);

            if (restrictionWithDifferentType is not null)
            {
                throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                    "All restrictions used to create a restriction session " +
                    "must have the same restriction type.",
                    new Dictionary<string, object?>
                    {
                        ["ExpectedRestrictionType"] = restrictionType,
                        ["ActualRestrictionType"] = restrictionWithDifferentType.RestrictionType,
                        ["RestrictionId"] = restrictionWithDifferentType.Id.Value
                    },
                    OperationType.Create
                );
            }

            var activeRestrictionIds = restrictions
                .Select(r => r.Id)
                .ToList();

            if (activeRestrictionIds.Distinct().Count() != restrictions.Count)
            {
                throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                    "A restriction session cannot contain duplicate restriction identifiers.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = userId.Value,
                        ["TotalCount"] = restrictions.Count
                    },
                    OperationType.Create
                );
            }

            var totalBlockedMinutes = restrictions
                .Sum(r => r.RestrictionRule.DurationMinutes);

            return new UserRestrictionSession(
                UserRestrictionSessionId.Create(),
                userId,
                activeRestrictionIds,
                restrictionType,
                totalBlockedMinutes,
                now
            );
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює сесію обмежень зі збереженого стану.
        ///
        /// (Restores a restriction session from its persisted state.)
        /// </summary>
        /// <param name="userRestrictionSessionId">Ідентифікатор сесії.</param>
        /// <param name="userId">Ідентифікатор власника сесії.</param>
        /// <param name="userRestrictionIds">
        /// Ідентифікатори активних обмежень сесії.
        /// </param>
        /// <param name="restrictionType">Тип обмежень сесії.</param>
        /// <param name="totalBlockedMinutes">
        /// Збережена сумарна тривалість обмежень у хвилинах.
        /// </param>
        /// <param name="createdAt">Час створення сесії.</param>
        /// <returns>Відновлена сесія обмежень.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язкові дані відсутні, тип не підтримується
        /// або сумарна тривалість не є додатною.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо набір містить дублікати ідентифікаторів.
        /// </exception>
        public static UserRestrictionSession Restore(
            UserRestrictionSessionId userRestrictionSessionId,
            UserId userId,
            IReadOnlyCollection<UserRestrictionId> userRestrictionIds,
            RestrictionType restrictionType,
            int totalBlockedMinutes,
            DateTimeOffset createdAt)
        {
            DomainGuard.AgainstNull<UserRestrictionSession>(
                OperationType.Restore,
                (userRestrictionSessionId, nameof(userRestrictionSessionId)),
                (userId, nameof(userId)),
                (userRestrictionIds, nameof(userRestrictionIds))
            );

            if (userRestrictionIds.Count == 0)
            {
                throw DomainDataInconsistencyException.Empty<UserRestrictionSession>(
                    nameof(userRestrictionIds),
                    OperationType.Restore
                );
            }

            if (userRestrictionIds.Any(id => id is null))
            {
                throw DomainDataInconsistencyException.Empty<UserRestrictionSession>(
                    nameof(userRestrictionIds),
                    OperationType.Restore
                );
            }

            var uniqueRestrictionIdCount = userRestrictionIds
                .Distinct()
                .Count();

            if(uniqueRestrictionIdCount != userRestrictionIds.Count)
            {
                throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                    "A restored restriction session cannot contain " +
                    "duplicate restriction identifiers.",
                    new Dictionary<string, object?>
                    {
                        ["TotalRestrictionIdCount"] = userRestrictionIds.Count,
                        ["UniqueRestrictionIdCount"] = uniqueRestrictionIdCount
                    },
                    OperationType.Restore
                );
            }

            DomainGuard.AgainstUndefinedEnum<UserRestrictionSession>(
                OperationType.Restore,
                (restrictionType, nameof(restrictionType))
            );
            
            if (totalBlockedMinutes <= 0)
            {
                throw DomainDataInconsistencyException.ValueOutOfRange<UserRestrictionSession>(
                    nameof(totalBlockedMinutes),
                    totalBlockedMinutes,
                    OperationType.Restore
                );
            }
                
            return new UserRestrictionSession(
                userRestrictionSessionId,
                userId,
                userRestrictionIds,
                restrictionType,
                totalBlockedMinutes,
                createdAt
            );
        }
        #endregion

        #region Queries
        /// <summary>
        /// Обчислює час завершення сесії.
        ///
        /// (Calculates the session expiration time.)
        /// </summary>
        /// <returns>
        /// Час створення сесії з доданою сумарною тривалістю обмежень.
        /// </returns>
        public DateTimeOffset GetExpirationDate()
        {
            return CreatedAt.AddMinutes(TotalBlockedMinutes);
        }

        /// <summary>
        /// Визначає, чи активна сесія у вказаний момент.
        ///
        /// (Determines whether the session is active at the specified time.)
        /// </summary>
        /// <param name="now">Час, відносно якого виконується перевірка.</param>
        /// <returns>
        /// true, якщо час не передує створенню та є меншим за час завершення;
        /// інакше false.
        /// </returns>
        public bool IsActive(DateTimeOffset now)
        {
            return now >= CreatedAt
                && now < GetExpirationDate();
        }

        /// <summary>
        /// Обчислює час, що залишився до завершення сесії.
        ///
        /// (Calculates the time remaining until the session expires.)
        /// </summary>
        /// <param name="now">Час, відносно якого виконується обчислення.</param>
        /// <returns>
        /// Залишок часу або <see cref="TimeSpan.Zero"/>, якщо сесія
        /// ще не почалася чи вже завершилася.
        /// </returns>
        public TimeSpan GetRemainingTime(DateTimeOffset now)
        {
            var expiration = GetExpirationDate();

            if (now < CreatedAt || now >= expiration)
                return TimeSpan.Zero;

            return expiration - now;
        }

        /// <summary>
        /// Визначає, чи завершилася сесія на вказаний момент
        /// у контексті внутрішньої доменної операції.
        ///
        /// Переданий час не може передувати часу створення сесії.
        ///
        /// (Determines whether the session has expired at the specified time
        /// in the context of an internal domain operation.
        ///
        /// The supplied time cannot precede the session creation time.)
        /// </summary>
        /// <param name="now">Час, відносно якого виконується перевірка.</param>
        /// <returns>
        /// true, якщо вказаний час дорівнює часу завершення
        /// сесії або перевищує його; інакше false.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо переданий час передує часу створення сесії.
        /// </exception>
        internal bool IsExpiredAt(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<UserRestrictionSession>(
                OperationType.Read,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            return now >= GetExpirationDate();
        }
        #endregion

        #region Behavior (Addition)
        /// <summary>
        /// Перевіряє можливість додавання обмежень до сесії
        /// без зміни її поточного стану.
        ///
        /// Успішне завершення методу означає, що обмеження
        /// можуть бути безпечно застосовані.
        ///
        /// (Verifies that restrictions can be added to the session
        /// without modifying its current state.
        ///
        /// Successful completion means that the restrictions
        /// can be safely applied.)
        /// </summary>
        /// <param name="userRestrictions">
        /// Обмеження, можливість додавання яких необхідно перевірити.
        /// </param>
        /// <param name="now">
        /// Час операції, який використовується для перевірки
        /// часової послідовності.
        /// </param>
        internal void ValidateCanAddRestrictions(
            IReadOnlyCollection<UserRestriction> userRestrictions,
            DateTimeOffset now)
        {
            _ = PrepareAddition(
                userRestrictions,
                now
            );
        }

        /// <summary>
        /// Додає попередньо перевірені активні обмеження до сесії.
        ///
        /// Перед мутацією повторно перевіряє та обчислює майбутній
        /// стан, після чого додає ідентифікатори й оновлює
        /// загальну тривалість блокування.
        ///
        /// (Adds pre-validated active restrictions to the session.
        ///
        /// Before mutation, revalidates and calculates the resulting state,
        /// then adds the identifiers and updates the total blocked duration.)
        /// </summary>
        /// <param name="userRestrictions">
        /// Активні обмеження, які необхідно додати.
        /// </param>
        /// <param name="now">
        /// Час операції, який використовується для повторної
        /// перевірки часової послідовності.
        /// </param>
        internal void AddRestrictions(
            IReadOnlyCollection<UserRestriction> userRestrictions,
            DateTimeOffset now)
        {
            var addition = PrepareAddition(
                userRestrictions,
                now
            );

            _activeRestrictionIds.UnionWith(addition.NewRestrictionIds);

            TotalBlockedMinutes = addition.UpdatedTotalBlockedMinutes;
        }

        /// <summary>
        /// Перевіряє передані обмеження та обчислює результат
        /// їх додавання без зміни стану сесії.
        ///
        /// Перевіряє активний статус, належність користувачу,
        /// відповідність типу сесії, відсутність повторних
        /// або вже доданих ідентифікаторів, а також те,
        /// що час операції не передує часу створення сесії.
        ///
        /// (Validates the supplied restrictions and calculates
        /// the result of adding them without mutating the session.
        ///
        /// Verifies active status, user ownership, session-type compatibility,
        /// the absence of duplicate or already included identifiers,
        /// and that the operation time is not earlier than
        /// the session creation time.)
        /// </summary>
        /// <param name="userRestrictions">
        /// Обмеження, які необхідно підготувати до додавання.
        /// </param>
        /// <param name="now">
        /// Час, відносно якого перевіряється хронологічна
        /// коректність операції.
        /// </param>
        /// <returns>
        /// Ідентифікатори нових обмежень і розрахована
        /// загальна тривалість блокування після їх додавання.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо колекція відсутня, порожня,
        /// містить null-елементи або час операції
        /// передує часу створення сесії.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження неактивне, належить іншому
        /// користувачу, має інший тип або дублює ідентифікатор.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Виникає, якщо сумарна тривалість виходить за межі Int32.
        /// </exception>
        private (
            IReadOnlyCollection<UserRestrictionId> NewRestrictionIds,
            int UpdatedTotalBlockedMinutes)
            PrepareAddition(
                IReadOnlyCollection<UserRestriction> userRestrictions,
                DateTimeOffset now)
        {
            DomainGuard.AgainstNull<UserRestrictionSession>(
                OperationType.Update,
                (userRestrictions, nameof(userRestrictions))
            );

            DomainGuard.AgainstEarlierThan<UserRestrictionSession>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            var restrictions = userRestrictions.ToList();

            if (restrictions.Count == 0)
            {
                throw DomainInvalidOperationException.PreconditionFailed<UserRestrictionSession>(
                    nameof(PrepareAddition),
                    "A non-empty restriction collection.",
                    OperationType.Update,
                    context: new Dictionary<string, object>
                    {
                        ["RestrictionCount"] = restrictions.Count
                    }
                );
            }

            if (restrictions.Any(x => x is null))
            {
                throw DomainInvalidOperationException.PreconditionFailed<UserRestrictionSession>(
                    nameof(PrepareAddition),
                    "The restriction collection contains a null value.",
                    OperationType.Update,
                    context: new Dictionary<string, object>
                    {
                        ["RestrictionCount"] = restrictions.Count,
                        ["NullRestrictionCount"] = restrictions.Count(x => x is null)
                    }
                );
            }

            var newRestrictionIds = new HashSet<UserRestrictionId>();
            var addedBlockedMinutes = 0;

            foreach (var restriction in restrictions)
            {
                if (restriction.Status != RestrictionStatus.Active)
                {
                    throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                        "Only active restrictions can be added to an active restriction session.",
                        new Dictionary<string, object?>
                        {
                            ["RestrictionId"] = restriction.Id.Value,
                            ["RestrictionStatus"] = restriction.Status
                        },
                        OperationType.Update
                    );
                }

                if (restriction.UserId != UserId)
                {
                    throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                        "A restriction belonging to another user cannot be added to this restriction session.",
                        new Dictionary<string, object?>
                        {
                            ["ExpectedUserId"] = UserId.Value,
                            ["ActualUserId"] = restriction.UserId.Value
                        },
                        OperationType.Update
                    );
                }

                if (restriction.RestrictionType != RestrictionType)
                {
                    throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                        "A restriction with a different type cannot be added to this restriction session.",
                        new Dictionary<string, object?>
                        {
                            ["ExpectedRestrictionType"] = RestrictionType,
                            ["ActualRestrictionType"] = restriction.RestrictionType
                        },
                        OperationType.Update
                    );
                }

                if (_activeRestrictionIds.Contains(restriction.Id))
                {
                    throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                        "One or more restrictions are already included in the session.",
                        new Dictionary<string, object?>
                        {
                            ["SessionId"] = Id.Value,
                            ["RestrictionId"] = restriction.Id.Value
                        },
                        OperationType.Update
                    );
                }

                if (!newRestrictionIds.Add(restriction.Id))
                {
                    throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                        "Restrictions being added contain duplicate identifiers.",
                        new Dictionary<string, object?>
                        {
                            ["SessionId"] = Id.Value,
                            ["DuplicateRestrictionId"] = restriction.Id.Value
                        },
                        OperationType.Update
                    );
                }

                addedBlockedMinutes = checked(addedBlockedMinutes + restriction.RestrictionRule.DurationMinutes);
            }

            var updatedTotalBlockedMinutes = checked(
                TotalBlockedMinutes + addedBlockedMinutes
            );

            return (
                newRestrictionIds,
                updatedTotalBlockedMinutes
            );
        }
        #endregion

        #region Behavior (Removal)
        /// <summary>
        /// Перевіряє передані обмеження та визначає наслідок
        /// їх вилучення для сесії у вказаний момент часу,
        /// не змінюючи її поточного стану.
        ///
        /// Метод розрізняє сесію, яка вже завершилася,
        /// і можливі результати вилучення: сесія залишиться
        /// активною, стане порожньою або завершиться через
        /// зменшення загальної тривалості.
        ///
        /// (Validates the supplied restrictions and determines
        /// the effect their removal would have on the session
        /// at the specified time without mutating its current state.
        ///
        /// Distinguishes an already expired session from the possible
        /// removal outcomes: remaining active, becoming empty,
        /// or becoming expired due to the reduced total duration.)
        /// </summary>
        /// <param name="userRestrictions">
        /// Обмеження, наслідок вилучення яких необхідно визначити.
        /// </param>
        /// <param name="now">
        /// Зафіксований час, відносно якого оцінюється стан сесії.
        /// </param>
        /// <returns>
        /// <see cref="RestrictionSessionRemovalEffect.AlreadyExpired"/>,
        /// якщо сесія вже завершилася;
        /// <see cref="RestrictionSessionRemovalEffect.BecomesEmpty"/>,
        /// якщо після вилучення в ній не залишиться обмежень;
        /// <see cref="RestrictionSessionRemovalEffect.BecomesExpired"/>,
        /// якщо після вилучення перерахований час завершення не пізніший за
        /// <paramref name="now"/>; інакше
        /// <see cref="RestrictionSessionRemovalEffect.RemainsActive"/>.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо колекція порожня або містить null-елементи.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження не належать сесії, мають інший тип,
        /// належать іншому користувачу, дублюються або розрахований
        /// стан сесії є неконсистентним.
        /// </exception>
        internal RestrictionSessionRemovalEffect ValidateCanRemoveRestrictions(
            IReadOnlyCollection<UserRestriction> userRestrictions,
            DateTimeOffset now)
        {
            var removal = PrepareRemoval(
                userRestrictions,
                now
            );

            if (IsExpiredAt(now))
                return RestrictionSessionRemovalEffect.AlreadyExpired;

            var remainingRestrictionCount = _activeRestrictionIds.Count - removal.RestrictionIdsToRemove.Count;

            if (remainingRestrictionCount == 0)
                return RestrictionSessionRemovalEffect.BecomesEmpty;

            var updatedExpirationDate = CreatedAt.AddMinutes(removal.UpdatedTotalBlockedMinutes);

            if (now >= updatedExpirationDate)
                return RestrictionSessionRemovalEffect.BecomesExpired;

            return RestrictionSessionRemovalEffect.RemainsActive;
        }

        /// <summary>
        /// Вилучає передані обмеження зі складу сесії.
        ///
        /// Перед мутацією повторно перевіряє вхідні дані
        /// та обчислює майбутній стан, після чого вилучає
        /// ідентифікатори й оновлює загальну тривалість блокування.
        ///
        /// Метод змінює лише стан сесії та не змінює
        /// статуси самих обмежень.
        ///
        /// (Removes the supplied restrictions from the session.
        ///
        /// Before mutation, revalidates the input and calculates
        /// the resulting state, then removes the identifiers and
        /// updates the total blocked duration.
        ///
        /// The method modifies only the session state and does not
        /// change the statuses of the restrictions themselves.)
        /// </summary>
        /// <param name="userRestrictions">Обмеження, які необхідно вилучити зі складу сесії.</param>
        /// <param name="now">Зафіксований час виконання операції.</param>
        internal void RemoveRestrictions(
            IReadOnlyCollection<UserRestriction> userRestrictions,
            DateTimeOffset now)
        {
            var removal = PrepareRemoval(
                userRestrictions,
                now
            );

            _activeRestrictionIds.ExceptWith(removal.RestrictionIdsToRemove);

            TotalBlockedMinutes = removal.UpdatedTotalBlockedMinutes;
        }

        /// <summary>
        /// Перевіряє передані обмеження та обчислює стан сесії
        /// після їх вилучення без виконання мутації.
        ///
        /// Формує унікальний набір ідентифікаторів, обчислює
        /// тривалість, яку необхідно відняти, та перевіряє,
        /// що отриманий стан сесії залишається консистентним.
        ///
        /// (Validates the supplied restrictions and calculates
        /// the session state after their removal without mutating it.
        ///
        /// Builds a unique identifier set, calculates the duration
        /// to subtract, and verifies that the resulting session state
        /// remains consistent.)
        /// </summary>
        /// <param name="userRestrictions">
        /// Обмеження, які необхідно підготувати до вилучення.
        /// </param>
        /// <param name="now">
        /// Зафіксований час виконання операції.
        /// </param>
        /// <returns>
        /// Ідентифікатори обмежень для вилучення та розрахована
        /// загальна тривалість блокування після операції.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо колекція порожня або містить null-елементи.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження не належить сесії, має інший тип,
        /// належить іншому користувачу, дублюється або призводить
        /// до неконсистентного стану сесії.
        /// </exception>
        private (
            IReadOnlyCollection<UserRestrictionId> RestrictionIdsToRemove,
            int UpdatedTotalBlockedMinutes)
            PrepareRemoval(
                IReadOnlyCollection<UserRestriction> userRestrictions,
                DateTimeOffset now)
        {
            DomainGuard.AgainstNull<UserRestrictionSession>(
                OperationType.Update,
                (userRestrictions, nameof(userRestrictions))
            );

            DomainGuard.AgainstEarlierThan<UserRestrictionSession>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            var restrictions = userRestrictions.ToArray();

            if (restrictions.Length == 0)
            {
                throw DomainInvalidOperationException.PreconditionFailed<UserRestrictionSession>(
                    nameof(PrepareRemoval),
                    "A non-empty restriction collection.",
                    OperationType.Update,
                    context: new Dictionary<string, object>
                    {
                        ["RestrictionCount"] = restrictions.Length
                    }
                );
            }

            if(restrictions.Any(x => x is null))
            {
                throw DomainInvalidOperationException.PreconditionFailed<UserRestrictionSession>(
                    nameof(PrepareRemoval),
                    "All restrictions must be non-null.",
                    OperationType.Update,
                    context: new Dictionary<string, object>
                    {
                        ["RestrictionCount"] = restrictions.Length,
                        ["NullRestrictionCount"] = restrictions.Count(x => x is null)
                    }
                );
            }

            var restrictionIdsToRemove = new HashSet<UserRestrictionId>();

            var totalMinutesToRemove = 0;

            foreach (var restriction in restrictions)
            {
                ValidateSingleRestriction(restriction);

                if (!restrictionIdsToRemove.Add(restriction.Id))
                {
                    throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                        "Restrictions being removed contain duplicate identifiers.",
                        new Dictionary<string, object?>
                        {
                            ["SessionId"] = Id.Value,
                            ["DuplicateRestrictionId"] = restriction.Id.Value
                        },
                        OperationType.Update
                    );
                }

                totalMinutesToRemove = checked(totalMinutesToRemove + restriction.RestrictionRule.DurationMinutes);
            }

            var updatedTotalBlockedMinutes = checked(TotalBlockedMinutes - totalMinutesToRemove);

            var remainingRestrictionCount = _activeRestrictionIds.Count - restrictionIdsToRemove.Count;

            EnsureSessionConsistency(
                remainingRestrictionCount,
                updatedTotalBlockedMinutes
            );

            return (
                restrictionIdsToRemove,
                updatedTotalBlockedMinutes
            );
        }

        /// <summary>
        /// Перевіряє належність одного обмеження поточній сесії.
        ///
        /// Обмеження повинно належати тому самому користувачу,
        /// мати відповідний тип і бути присутнім серед активних
        /// ідентифікаторів сесії.
        ///
        /// (Validates that a single restriction belongs to the current session.
        ///
        /// The restriction must belong to the same user, have the matching type,
        /// and be present among the session’s active identifiers.)
        /// </summary>
        /// <param name="userRestriction">
        /// Обмеження, належність якого необхідно перевірити.
        /// </param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження не відповідає стану сесії.
        /// </exception>
        private void ValidateSingleRestriction(UserRestriction userRestriction)
        {
            DomainGuard.AgainstNull<UserRestrictionSession>(
                OperationType.Update,
                (userRestriction, nameof(userRestriction))
            );

            if (userRestriction.UserId != UserId)
            {
                throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                    "A restriction belonging to another user cannot be removed from this restriction session.",
                    new Dictionary<string, object?>
                    {
                        ["CurrentUserId"] = UserId,
                        ["TargetUserId"] = userRestriction.UserId
                    },
                    OperationType.Update
                );
            }

            if (userRestriction.RestrictionType != RestrictionType)
            {
                throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                    "A restriction with a different type cannot be removed from this restriction session.",
                    new Dictionary<string, object?>
                    {
                        ["CurrentRestrictionType"] = RestrictionType,
                        ["TargetRestrictionType"] = userRestriction.RestrictionType
                    },
                    OperationType.Update
                );
            }

            if (!_activeRestrictionIds.Contains(userRestriction.Id))
            {
                throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                    "The restriction session does not contain the restriction being removed.",
                    new Dictionary<string, object?>
                    {
                        ["SessionId"] = Id.Value,
                        ["RestrictionId"] = userRestriction.Id.Value
                    },
                    OperationType.Update
                );
            }
        }

        /// <summary>
        /// Перевіряє консистентність розрахованого стану сесії.
        ///
        /// Порожня сесія повинна мати нульову тривалість,
        /// а непорожня — додатну тривалість блокування.
        ///
        /// (Validates the consistency of the calculated session state.
        ///
        /// An empty session must have zero duration, while a non-empty
        /// session must have a positive blocked duration.)
        /// </summary>
        /// <param name="remainingRestrictionCount">
        /// Кількість обмежень, що залишаться в сесії.
        /// </param>
        /// <param name="updatedTotalBlockedMinutes">
        /// Загальна тривалість блокування після вилучення.
        /// </param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо кількість обмежень і тривалість
        /// утворюють неконсистентний стан.
        /// </exception>
        private void EnsureSessionConsistency(
            int remainingRestrictionCount,
            int updatedTotalBlockedMinutes)
        {
            bool hasConsistentResult = (remainingRestrictionCount, updatedTotalBlockedMinutes) switch
            {
                (0, 0) => true,
                (> 0, > 0) => true,
                _ => false
            };

            if (!hasConsistentResult)
            {
                throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                    "Removing the restrictions would produce an inconsistent restriction session.",
                    new Dictionary<string, object?>
                    {
                        ["CurrentTotalBlockedMinutes"] = TotalBlockedMinutes,
                        ["UpdatedTotalBlockedMinutes"] = updatedTotalBlockedMinutes,
                        ["RemainingRestrictionCount"] = remainingRestrictionCount
                    },
                    OperationType.Update
                );
            }
        }
        #endregion

        #region Behavior
        /// <summary>
        /// Перевіряє, чи містить сесія точно переданий набір
        /// ідентифікаторів активних обмежень.
        ///
        /// Порівнює як склад множини, так і кількість елементів,
        /// тому дублікати також вважаються невідповідністю.
        ///
        /// (Checks whether the session contains exactly the supplied set
        /// of active restriction identifiers.
        ///
        /// Compares both set membership and element count,
        /// so duplicates are also treated as a mismatch.)
        /// </summary>
        /// <param name="restrictionIds">Ідентифікатори обмежень для порівняння.</param>
        /// <returns>
        /// true, якщо набори ідентифікаторів повністю збігаються; інакше false.
        /// </returns>
        internal bool ContainsExactly(IEnumerable<UserRestrictionId> restrictionIds)
        {
            DomainGuard.AgainstNull<UserRestrictionSession>(
                OperationType.Read,
                (restrictionIds, nameof(restrictionIds))
            );

            var identifiers = restrictionIds.ToList();

            if (identifiers.Any(id => id is null))
            {
                throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                    "The restriction identifier collection contains a null value.",
                    new Dictionary<string, object?>
                    {
                        ["RestrictionIdentifierCount"] = identifiers.Count
                    },
                    OperationType.Read
                );
            }

            return identifiers.Count == _activeRestrictionIds.Count &&
                _activeRestrictionIds.SetEquals(identifiers);
        }

        internal bool ContainsRestriction(UserRestrictionId restrictionId)
        {
            DomainGuard.AgainstNull<UserRestrictionSession>(
                OperationType.Read,
                (restrictionId, nameof(restrictionId))
            );

            return _activeRestrictionIds.Contains(restrictionId);
        }

        /// <summary>
        /// Визначає, чи не залишилося в сесії активних обмежень.
        ///
        /// (Determines whether the session contains no active restrictions.)
        /// </summary>
        /// <returns>true, якщо сесія порожня; інакше false.</returns>
        internal bool IsEmpty()
            => _activeRestrictionIds.Count == 0;
        #endregion
    }
}
