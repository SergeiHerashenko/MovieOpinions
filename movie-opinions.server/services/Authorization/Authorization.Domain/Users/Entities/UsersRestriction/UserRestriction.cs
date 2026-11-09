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
        /// Ім’я або позначення ініціатора накладення обмеження:
        /// адміністратора чи системи.
        ///
        /// (Name or designation of the restriction issuer:
        /// an administrator or the system.)
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

        /// <summary>
        /// Перевіряє вхідні дані та створює нове активне обмеження користувача.
        ///
        /// (Validates the input and creates a new active user restriction.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача, на якого накладається обмеження.
        ///
        /// (Identifier of the user on whom the restriction is imposed.)
        /// </param>
        /// <param name="restrictionType">
        /// Тип обмеження.
        ///
        /// (Type of restriction.)
        /// </param>
        /// <param name="restrictionRule">
        /// Правило, що визначає назву та тривалість обмеження.
        ///
        /// (Rule defining the restriction name and duration.)
        /// </param>
        /// <param name="imposedBy">
        /// Ім’я або позначення ініціатора накладення обмеження.
        ///
        /// (Name or designation of the restriction issuer.)
        /// </param>
        /// <param name="reason">
        /// Необов’язкова причина накладення обмеження.
        ///
        /// (Optional reason for imposing the restriction.)
        /// </param>
        /// <param name="now">
        /// Час створення обмеження.
        ///
        /// (Restriction creation time.)
        /// </param>
        /// <returns>
        /// Створене активне обмеження.
        ///
        /// (The newly created active restriction.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язкові вхідні дані відсутні
        /// або тип обмеження не підтримується.
        ///
        /// (Thrown when required input values are missing
        /// or the restriction type is unsupported.)
        /// </exception>
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

        /// <summary>
        /// Перевіряє збережені дані та відновлює стан обмеження користувача.
        ///
        /// (Validates persisted data and restores the user restriction state.)
        /// </summary>
        /// <param name="userRestrictionId">
        /// Збережений ідентифікатор обмеження.
        ///
        /// (The persisted restriction identifier.)
        /// </param>
        /// <param name="userId">
        /// Ідентифікатор користувача, якому належить обмеження.
        ///
        /// (Identifier of the user who owns the restriction.)
        /// </param>
        /// <param name="restrictionRule">
        /// Відновлене правило обмеження.
        ///
        /// (The restored restriction rule.)
        /// </param>
        /// <param name="restrictionType">
        /// Збережений тип обмеження.
        ///
        /// (The persisted restriction type.)
        /// </param>
        /// <param name="imposedBy">
        /// Збережене ім’я або позначення ініціатора накладення обмеження.
        ///
        /// (The persisted name or designation of the restriction issuer.)
        /// </param>
        /// <param name="reason">
        /// Збережена необов’язкова причина накладення обмеження.
        ///
        /// (The persisted optional reason for imposing the restriction.)
        /// </param>
        /// <param name="restrictionStatus">
        /// Збережений статус обмеження.
        ///
        /// (The persisted restriction status.)
        /// </param>
        /// <param name="createdAt">
        /// Збережений час створення обмеження.
        ///
        /// (The persisted restriction creation time.)
        /// </param>
        /// <param name="revokedAt">
        /// Збережений час дострокового зняття або null,
        /// якщо обмеження не перебуває у стані Revoked.
        ///
        /// (The persisted revocation time, or null
        /// when the restriction is not in the Revoked state.)
        /// </param>
        /// <returns>
        /// Відновлене обмеження користувача.
        ///
        /// (The restored user restriction.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язкові збережені дані відсутні,
        /// значення переліків не підтримуються або час зняття
        /// передує часу створення обмеження.
        ///
        /// (Thrown when required persisted values are missing,
        /// enum values are unsupported, or the revocation time
        /// precedes the restriction creation time.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо статус обмеження не узгоджується
        /// з наявністю часу дострокового зняття.
        ///
        /// (Thrown when the restriction status is inconsistent
        /// with the presence of a revocation timestamp.)
        /// </exception>
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
        /// (Checks the preconditions for a system operation that marks
        /// an active restriction as completed, without modifying its state.
        ///
        /// A non-active status in this internal scenario
        /// is treated as a domain-state violation.)
        /// </summary>
        /// <param name="now">
        /// Час виконання перевірки.
        ///
        /// (Time at which the validation is performed.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо переданий час передує часу створення обмеження
        /// або поточний статус не підтримується доменною моделлю.
        ///
        /// (Thrown when the supplied time precedes the restriction creation
        /// time or the current status is unsupported by the domain model.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження вже не є активним.
        ///
        /// (Thrown when the restriction is no longer active.)
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
            {
                return;
            }

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
        /// (Marks an active restriction as completed
        /// as part of a system operation.
        ///
        /// Revalidates the internal transition preconditions
        /// before modifying the state.)
        /// </summary>
        /// <param name="now">
        /// Час завершення обмеження.
        ///
        /// (Restriction completion time.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо переданий час передує часу створення обмеження
        /// або поточний чи цільовий статус не підтримується.
        ///
        /// (Thrown when the supplied time precedes the restriction
        /// creation time or the current or target status is unsupported.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження вже не є активним.
        ///
        /// (Thrown when the restriction is no longer active.)
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
        /// <param name="now">
        /// Час виконання перевірки.
        ///
        /// (Time at which the validation is performed.)
        /// </param>
        /// <returns>
        /// Успішний результат, якщо обмеження активне;
        /// інакше очікувана помилка поточного стану.
        ///
        /// (A successful result when the restriction is active;
        /// otherwise, an expected error describing its current state.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо переданий час передує часу створення обмеження
        /// або поточний статус не підтримується доменною моделлю.
        ///
        /// (Thrown when the supplied time precedes the restriction creation
        /// time or the current status is unsupported by the domain model.)
        /// </exception>
        internal Result ValidateRevocation(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<UserRestriction>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            if (Status == RestrictionStatus.Active)
            {
                return Result.Success();
            }

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
        /// <param name="now">
        /// Час дострокового зняття обмеження.
        ///
        /// (Restriction revocation time.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо переданий час передує часу створення
        /// обмеження або поточний чи цільовий статус не підтримується.
        ///
        /// (Thrown when the supplied time precedes the restriction creation
        /// time or the current or target status is unsupported.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо обмеження вже не є активним.
        ///
        /// (Thrown when the restriction is no longer active.)
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
        /// <param name="targetStatus">
        /// Цільовий статус обмеження.
        ///
        /// (The target restriction status.)
        /// </param>
        /// <param name="now">
        /// Час виконання переходу.
        ///
        /// (Time at which the transition is performed.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо переданий час передує часу створення обмеження, поточний
        /// або цільовий статус не підтримується чи цільовий статус не дозволений
        /// для цього переходу.
        ///
        /// (Thrown when the supplied time precedes the restriction creation time,
        /// the current or target status is unsupported, or the target status is not
        /// permitted for this transition.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо поточне обмеження вже не є активним.
        ///
        /// (Thrown when the current restriction is no longer active.)
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

            if (Status != RestrictionStatus.Active)
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
