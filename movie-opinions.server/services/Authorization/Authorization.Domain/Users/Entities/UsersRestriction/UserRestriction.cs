using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.Entities.UsersRestriction.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.Errors;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Entities.UsersRestriction
{
    /// <summary>
    /// Представляє окремий історичний факт накладення обмеження на користувача.
    /// Зберігає правило, тип, ініціатора та поточний стан обмеження.
    /// Загальний строк блокування визначається сесією обмежень.
    ///
    /// (Represents an individual historical fact of imposing a restriction on a user.
    /// Stores the rule, type, issuer, and current state of the restriction.
    /// The overall blocking period is determined by the restriction session.)
    /// </summary>
    public sealed class UserRestriction : Entity<UserRestrictionId>, IUserOwned
    {
        #region Properties
        /// <summary>
        /// Ідентифікатор користувача, на якого накладено обмеження.
        ///
        /// (Identifier of the user on whom the restriction was imposed.)
        /// </summary>
        public UserId UserId { get; }

        /// <summary>
        /// Правило, яке визначає назву та тривалість обмеження.
        ///
        /// (Rule defining the restriction name and duration.)
        /// </summary>
        public RestrictionRule RestrictionRule { get; }

        /// <summary>
        /// Тип накладеного обмеження.
        ///
        /// (Type of the imposed restriction.)
        /// </summary>
        public RestrictionType RestrictionType { get; }

        /// <summary>
        /// Необов’язкова причина накладення обмеження.
        ///
        /// (Optional reason for imposing the restriction.)
        /// </summary>
        public string? Reason { get; }

        // TODO: Після моделювання адміністраторів переглянути ImposedBy:
        // визначити, чи зберігати AdminId, знімок імені адміністратора
        // або обидва значення.

        /// <summary>
        /// Ім’я або нік адміністратора, який наклав обмеження.
        ///
        /// (Name or nickname of the administrator who imposed the restriction.)
        /// </summary>
        public string ImposedBy { get; }

        /// <summary>
        /// Поточний стан обмеження.
        ///
        /// (Current state of the restriction.)
        /// </summary>
        public RestrictionStatus Status { get; private set; }

        /// <summary>
        /// Дата й час дострокового зняття обмеження.
        /// Має значення лише для стану Revoked.
        ///
        /// (Date and time when the restriction was revoked early.
        /// Has a value only when the restriction is in the Revoked state.)
        /// </summary>
        public DateTimeOffset? RevokedAt { get; private set; }
        #endregion

        #region Creation
        /// <summary>
        /// Створює нове активне обмеження користувача.
        ///
        /// (Creates a new active user restriction.)
        /// </summary>
        /// <returns>Створене активне обмеження.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо внутрішні вхідні дані відсутні або некоректні.
        /// </exception>
        private UserRestriction(
            UserRestrictionId userRestrictionId,
            UserId userId,
            RestrictionRule restrictionRule,
            RestrictionType restrictionType,
            string imposedBy,
            string? reason,
            DateTimeOffset now)
            : base(userRestrictionId, now)
        {
            UserId = userId;
            RestrictionRule = restrictionRule;
            RestrictionType = restrictionType;
            Reason = reason;
            ImposedBy = imposedBy;
            Status = RestrictionStatus.Active;
            RevokedAt = null;
        }

        internal static UserRestriction Create(
            UserId userId,
            RestrictionType restrictionType,
            RestrictionRule restrictionRule,
            string imposedBy,
            string? reason,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<UserRestriction>(
                OperationType.Create,
                (userId, nameof(userId)),
                (restrictionRule, nameof(restrictionRule))
            );

            DomainGuard.AgainstUndefinedEnum<UserRestriction>(
                OperationType.Create,
                (restrictionType, nameof(restrictionType))
            );

            if (string.IsNullOrWhiteSpace(imposedBy))
            {
                throw DomainDataInconsistencyException.Empty<UserRestriction>(
                    nameof(imposedBy),
                    OperationType.Create
                );
            }

            return new UserRestriction(
                UserRestrictionId.Create(),
                userId,
                restrictionRule,
                restrictionType,
                imposedBy,
                reason,
                now
            );
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює обмеження зі збереженого стану.
        ///
        /// (Restores a restriction from its persisted state.)
        /// </summary>
        /// <returns>Відновлене обмеження.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережені дані містять порожні значення,
        /// невідомі enum або некоректні часові значення.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо статус не узгоджується з часом дострокового зняття.
        /// </exception>
        private UserRestriction(
            UserRestrictionId userRestrictionId,
            UserId userId,
            RestrictionRule restrictionRule,
            RestrictionType restrictionType,
            string imposedBy,
            string? reason,
            RestrictionStatus restrictionStatus,
            DateTimeOffset createdAt,
            DateTimeOffset? revokedAt)
            : base(userRestrictionId, createdAt)
        {
            UserId = userId;
            RestrictionType = restrictionType;
            RestrictionRule = restrictionRule;
            Reason = reason;
            ImposedBy = imposedBy;
            Status = restrictionStatus;
            RevokedAt = revokedAt;
        }

        public static UserRestriction Restore(
            UserRestrictionId userRestrictionId,
            UserId userId,
            RestrictionRule restrictionRule,
            RestrictionType restrictionType,
            string imposedBy,
            string? reason,
            RestrictionStatus restrictionStatus,
            DateTimeOffset createdAt,
            DateTimeOffset? revokedAt)
        {
            DomainGuard.AgainstNull<UserRestriction>(
                OperationType.Restore,
                (userRestrictionId, nameof(userRestrictionId)),
                (userId, nameof(userId)),
                (restrictionRule, nameof(restrictionRule))
            );

            if (string.IsNullOrWhiteSpace(imposedBy))
            {
                throw DomainDataInconsistencyException.Empty<UserRestriction>(
                    nameof(imposedBy),
                    OperationType.Restore
                );
            }

            ValidateState(
                restrictionType,
                restrictionStatus,
                createdAt,
                revokedAt,
                OperationType.Restore
            );

            return new UserRestriction(
                userRestrictionId,
                userId,
                restrictionRule,
                restrictionType,
                imposedBy,
                reason,
                restrictionStatus,
                createdAt,
                revokedAt
            );
        }
        #endregion

        #region Behavior (Complete)
        /// <summary>
        /// Перевіряє, чи може активне обмеження системно перейти
        /// у завершений стан, не змінюючи його.
        /// 
        /// Неактивний статус у цьому внутрішньому сценарії
        /// вважається порушенням доменного стану.
        /// 
        /// (Verifies that an active restriction can systematically
        /// transition to the completed state without modifying it.
        /// 
        /// A non-active status in this internal scenario
        /// is treated as a domain-state violation.)
        /// </summary>
        /// <param name="now">Час виконання перевірки.</param>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо переданий час передує часу створення обмеження.
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо поточний статус не підтримується доменною моделлю.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження вже не є активним.
        /// </exception>
        internal void ValidateCanComplete(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<UserRestriction>(
               OperationType.Update,
               (now, nameof(now)),
               (CreatedAt, nameof(CreatedAt))
            );

            DomainGuard.AgainstUndefinedEnum<UserRestriction>(
                OperationType.Update,
                (Status, nameof(Status))
            );

            if (Status == RestrictionStatus.Active)
                return;

            throw DomainInvariantViolationException.BrokenState<UserRestriction>(
                 "Only an active restriction can transition to the 'Completed' state.",
                new Dictionary<string, object?>
                {
                    ["RestrictionId"] = Id.Value,
                    ["RestrictionType"] = RestrictionType,
                    ["RestrictionStatus"] = Status
                },
                OperationType.Update
            );
        }

        /// <summary>
        /// Системно переводить активне обмеження у завершений стан.
        /// 
        /// Перед зміною повторно перевіряє внутрішні передумови переходу.
        /// 
        /// (Systematically transitions an active restriction
        /// to the completed state.
        /// 
        /// Revalidates the internal transition preconditions
        /// before modifying the state.)
        /// </summary>
        /// <param name="now">Час завершення обмеження.</param>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо переданий час передує часу створення обмеження.
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо поточний або цільовий статус не підтримується.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження вже не є активним.
        /// </exception>
        internal void MarkAsCompleted(DateTimeOffset now)
        {
            TransitionFromActive(
                RestrictionStatus.Completed,
                now
            );
        }
        #endregion

        #region Behavior (Revoke)
        /// <summary>
        /// Перевіряє можливість дострокового зняття обмеження
        /// без зміни його поточного стану.
        /// 
        /// Завершене або вже зняте обмеження повертає
        /// очікувану доменну помилку.
        /// 
        /// (Verifies that the restriction can be revoked early
        /// without modifying its current state.
        /// 
        /// A completed or already revoked restriction returns
        /// an expected domain error.)
        /// </summary>
        /// <param name="now">Час виконання перевірки.</param>
        /// <returns>
        /// Успішний результат, якщо обмеження активне;
        /// інакше очікувана помилка поточного стану.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо переданий час передує часу створення обмеження.
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо поточний статус не підтримується доменною моделлю.
        /// </exception>
        internal Result ValidateRevocation(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<UserRestriction>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            if (Status == RestrictionStatus.Active)
                return Result.Success();

            Error error = Status switch
            {
                RestrictionStatus.Completed =>
                    RestrictionErrors.AlreadyCompleted<UserRestriction>(),

                RestrictionStatus.Revoked =>
                    RestrictionErrors.AlreadyRevoked<UserRestriction>(),

                _ => throw DomainDataInconsistencyException.UnsupportedDiscriminator<UserRestriction>(
                    nameof(Status),
                    Status,
                    OperationType.Update
                )
            };

            return Result.Failure(error);
        }

        /// <summary>
        /// Достроково знімає активне обмеження
        /// та записує час його зняття.
        /// 
        /// Перед зміною повторно перевіряє внутрішні передумови переходу.
        /// 
        /// (Revokes an active restriction early
        /// and records its revocation time.
        /// 
        /// Revalidates the internal transition preconditions
        /// before modifying the state.)
        /// </summary>
        /// <param name="now">Час дострокового зняття обмеження.</param>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо переданий час передує часу створення обмеження.
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо поточний або цільовий статус не підтримується.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження вже не є активним.
        /// </exception>
        internal void RevokeRestriction(DateTimeOffset now)
        {
            TransitionFromActive(
                RestrictionStatus.Revoked,
                now
            );
        }
        #endregion

        #region Behavior
        /// <summary>
        /// Виконує внутрішній перехід активного обмеження
        /// у завершений або достроково знятий стан.
        /// 
        /// Метод приймає лише <see cref="RestrictionStatus.Completed"/>
        /// та <see cref="RestrictionStatus.Revoked"/> як цільові статуси.
        /// 
        /// (Performs the internal transition of an active restriction
        /// to the completed or revoked state.
        /// 
        /// The method accepts only <see cref="RestrictionStatus.Completed"/>
        /// and <see cref="RestrictionStatus.Revoked"/> as target statuses.)
        /// </summary>
        /// <param name="targetStatus">Цільовий статус обмеження.</param>
        /// <param name="now">Час виконання переходу.</param>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо переданий час передує часу створення обмеження.
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо поточний або цільовий статус не підтримується
        /// чи не може бути ціллю цього переходу.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо поточне обмеження вже не є активним.
        /// </exception>
        private void TransitionFromActive(
            RestrictionStatus targetStatus,
            DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<UserRestriction>(
               OperationType.Update,
               (now, nameof(now)),
               (CreatedAt, nameof(CreatedAt))
           );

            DomainGuard.AgainstUndefinedEnum<UserRestriction>(
                OperationType.Update,
                (targetStatus, nameof(targetStatus)),
                (Status, nameof(Status))
            );

            if(Status != RestrictionStatus.Active)
            {
                throw DomainInvariantViolationException.BrokenState<UserRestriction>(
                    $"Only an active restriction can transition to the '{targetStatus}' state.",
                    new Dictionary<string, object?>
                    {
                        ["RestrictionId"] = Id.Value,
                        ["RestrictionType"] = RestrictionType,
                        ["RestrictionStatus"] = Status,
                        ["TargetStatus"] = targetStatus
                    },
                    OperationType.Update
                );
            }

            switch (targetStatus)
            {
                case RestrictionStatus.Revoked:
                    Status = RestrictionStatus.Revoked;
                    RevokedAt = now;
                    break;

                case RestrictionStatus.Completed:
                    Status = RestrictionStatus.Completed;
                    break;

                default:
                    throw DomainDataInconsistencyException.ValueOutOfRange<UserRestriction>(
                        nameof(targetStatus),
                        targetStatus,
                        OperationType.Update
                    );
            }
        }
        #endregion

        #region Guard
        /// <summary>
        /// Перевіряє консистентність відновленого стану обмеження.
        ///
        /// (Validates the consistency of the restored restriction state.)
        /// </summary>
        private static void ValidateState(
            RestrictionType restrictionType,
            RestrictionStatus restrictionStatus,
            DateTimeOffset createdAt,
            DateTimeOffset? revokedAt,
            OperationType operationType)
        {
            DomainGuard.AgainstUndefinedEnum<UserRestriction>(
                operationType,
                (restrictionType, nameof(restrictionType)),
                (restrictionStatus, nameof(restrictionStatus))
            );

            if (revokedAt is not null)
            {
                DomainGuard.AgainstEarlierThan<UserRestriction>(
                    operationType,
                    (revokedAt.Value, nameof(revokedAt)),
                    (createdAt, nameof(createdAt))
                );
            }

            bool isValidState = (restrictionStatus, revokedAt) switch
            {
                (RestrictionStatus.Active, null) => true,
                (RestrictionStatus.Completed, null) => true,
                (RestrictionStatus.Revoked, not null) => true,
                _ => false
            };

            if (!isValidState)
            {
                throw DomainInvariantViolationException.BrokenState<UserRestriction>(
                    $"Inconsistent restriction state: " +
                    $"fields '{nameof(revokedAt)}' and '{nameof(restrictionStatus)}' are not consistent!",
                    new Dictionary<string, object?>
                    {
                        ["RestrictionStatus"] = restrictionStatus,
                        ["RevokedAt"] = revokedAt
                    },
                    operationType
                );
            }
        }
        #endregion
    }
}
