using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
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
    public sealed class UserRestriction : Entity<UserRestrictionId>
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
            DateTimeOffset now,
            string? reason = null)
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

        #region Behavior
        /// <summary>
        /// Достроково знімає активне обмеження.
        ///
        /// (Revokes an active restriction before its natural completion.)
        /// </summary>
        /// <param name="now">Час виконання операції.</param>
        /// <returns>
        /// Успішний результат або помилка, якщо обмеження вже завершене чи зняте.
        /// </returns>
        internal Result RevokeRestriction(DateTimeOffset now)
        {
            return TransitionFromActive(
                now,
                RestrictionStatus.Revoked
            );
        }

        /// <summary>
        /// Позначає активне обмеження як повністю відбуте.
        ///
        /// (Marks an active restriction as fully completed.)
        /// </summary>
        /// <param name="now">Час виконання операції.</param>
        /// <returns>
        /// Успішний результат або помилка, якщо обмеження вже завершене чи зняте.
        /// </returns>
        /// <remarks>
        /// Рішення про завершення строку приймає сесія обмежень.
        ///
        /// (The restriction session determines whether the restriction period
        /// has been completed.)
        /// </remarks>
        internal Result MarkAsCompleted(DateTimeOffset now)
        {
            return TransitionFromActive(
                now,
                RestrictionStatus.Completed
            );
        }

        /// <summary>
        /// Виконує дозволений перехід активного обмеження
        /// у завершений або достроково знятий стан.
        ///
        /// (Performs an allowed transition of an active restriction
        /// to either the completed or revoked state.)
        /// </summary>
        /// <param name="now">Час виконання переходу.</param>
        /// <param name="targetStatus">Цільовий кінцевий статус обмеження.</param>
        /// <returns>
        /// Успішний результат або помилка, якщо обмеження вже перебуває
        /// в кінцевому стані.
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції некоректний або передано
        /// непідтримуваний поточний чи цільовий статус.
        /// </exception>
        private Result TransitionFromActive(
            DateTimeOffset now,
            RestrictionStatus targetStatus)
        {
            DomainGuard.AgainstEarlierThan<UserRestriction>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            if (Status != RestrictionStatus.Active)
            {
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

            return Result.Success();
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
