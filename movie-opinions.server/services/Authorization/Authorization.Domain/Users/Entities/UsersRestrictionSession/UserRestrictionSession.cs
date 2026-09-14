using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
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
    public sealed class UserRestrictionSession : Entity<UserRestrictionSessionId>
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
        #endregion

        #region Behavior
        /// <summary>
        /// Додає до сесії нові обмеження та збільшує
        /// її сумарну тривалість.
        ///
        /// (Adds new restrictions to the session and increases
        /// its total duration.)
        /// </summary>
        /// <param name="userRestrictions">Обмеження, які потрібно додати.</param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо колекція відсутня, порожня або містить null.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження належать іншому користувачу,
        /// мають інший тип, уже входять до сесії або містять дублікати.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Виникає, якщо нова сумарна тривалість перевищує діапазон Int32.
        /// </exception>
        internal void AddRestrictions(IEnumerable<UserRestriction> userRestrictions)
        {
            DomainGuard.AgainstNull<UserRestrictionSession>(
                OperationType.Update,
                (userRestrictions, nameof(userRestrictions))
            );

            var restrictions = userRestrictions.ToList();

            if (restrictions.Count == 0)
            {
                throw DomainDataInconsistencyException.Empty<UserRestrictionSession>(
                    nameof(userRestrictions),
                    OperationType.Update
                );
            }

            var newRestrictionIds = new HashSet<UserRestrictionId>();
            var addedBlockedMinutes = 0;

            foreach (var restriction in restrictions)
            {
                if (restriction is null)
                {
                    throw DomainDataInconsistencyException.Empty<UserRestrictionSession>(
                        nameof(userRestrictions),
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

            var updatedTotalBlockedMinutes = checked(TotalBlockedMinutes + addedBlockedMinutes);

            _activeRestrictionIds.UnionWith(newRestrictionIds);
            TotalBlockedMinutes = updatedTotalBlockedMinutes;
        }

        /// <summary>
        /// Видаляє обмеження із сесії та зменшує
        /// її сумарну тривалість.
        ///
        /// (Removes a restriction from the session and decreases
        /// its total duration.)
        /// </summary>
        /// <param name="userRestriction">Обмеження, яке потрібно видалити.</param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обмеження відсутнє.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження не належить цій сесії
        /// або його видалення створює неузгоджений стан.
        /// </exception>
        internal void RemoveRestriction(UserRestriction userRestriction)
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

            var updatedTotalBlockedMinutes = TotalBlockedMinutes - userRestriction.RestrictionRule.DurationMinutes;

            var remainingRestrictionCount = _activeRestrictionIds.Count - 1;

            bool hasConsistentResult = (remainingRestrictionCount, updatedTotalBlockedMinutes) switch
            {
                (0, 0) => true,
                (> 0, > 0) => true,
                _ => false
            };

            if (!hasConsistentResult)
            {
                throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                    "Removing the restriction would produce an inconsistent restriction session.",
                    new Dictionary<string, object?>
                    {
                        ["CurrentTotalBlockedMinutes"] = TotalBlockedMinutes,
                        ["RestrictionDurationMinutes"] = userRestriction.RestrictionRule.DurationMinutes,
                        ["RemainingRestrictionCount"] = remainingRestrictionCount,
                        ["RestrictionId"] = userRestriction.Id.Value
                    },
                    OperationType.Update
                );
            }

            _activeRestrictionIds.Remove(userRestriction.Id);
            TotalBlockedMinutes = updatedTotalBlockedMinutes;
        }

        /// <summary>
        /// Визначає, чи містить сесія обмеження із заданим ідентифікатором.
        ///
        /// (Determines whether the session contains a restriction
        /// with the specified identifier.)
        /// </summary>
        /// <param name="restrictionId">Ідентифікатор обмеження.</param>
        /// <returns>true, якщо обмеження входить до сесії; інакше false.</returns>
        internal bool ContainsRestriction(UserRestrictionId restrictionId)
        {
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
