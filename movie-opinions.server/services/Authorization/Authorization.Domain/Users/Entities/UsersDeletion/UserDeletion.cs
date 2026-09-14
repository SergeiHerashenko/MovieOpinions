using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersDeletion.Errors;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.Entities.UsersDeletion
{
    /// <summary>
    /// Дочірня сутність агрегату User, яка зберігає інформацію
    /// про видалення користувача та керує його життєвим циклом.
    /// Підтримує переходи зі стану Deleted у Restored
    /// або PermanentlyDeleted.
    ///
    /// (Child entity of the User aggregate that stores user deletion information
    /// and controls its lifecycle. Supports transitions from Deleted
    /// to Restored or PermanentlyDeleted.)
    /// </summary>
    public sealed class UserDeletion : Entity<UserDeletionId>
    {
        private const int RESTORATION_PERIOD_IN_DAYS = 30;

        #region Properties
        /// <summary>
        /// Ідентифікатор користувача, якому належить запис про видалення.
        ///
        /// (Identifier of the user that owns this deletion record.)
        /// </summary>
        public UserId UserId { get; }

        /// <summary>
        /// Знімок логіна користувача на момент створення запису про видалення.
        ///
        /// (Snapshot of the user's login when the deletion record was created.)
        /// </summary>
        public Login Login { get; }

        /// <summary>
        /// Нормалізована причина видалення користувача.
        ///
        /// (Normalized reason for deleting the user.)
        /// </summary>
        public DeletionReason Reason { get; }

        /// <summary>
        /// Момент, до якого включно користувача можна відновити.
        ///
        /// (The moment until which the user can be restored, inclusively.)
        /// </summary>
        public DateTimeOffset RestoreUntil { get; }

        /// <summary>
        /// Час відновлення користувача або null, якщо відновлення не відбулося.
        ///
        /// (Time when the user was restored, or null if restoration did not occur.)
        /// </summary>
        public DateTimeOffset? RestoredAt { get; private set; }

        /// <summary>
        /// Поточний стан життєвого циклу видалення.
        ///
        /// (Current state of the deletion lifecycle.)
        /// </summary>
        public DeletionStatus Status { get; private set; }

        /// <summary>
        /// Час останньої зміни стану видалення.
        /// Для початкового стану Deleted має значення null.
        ///
        /// (Time of the most recent deletion-state transition.
        /// Null for the initial Deleted state.)
        /// </summary>
        public DateTimeOffset? UpdatedAt { get; private set; }
        #endregion

        #region Creation
        /// <summary>
        /// Створює запис про видалення користувача у стані Deleted
        /// та відкриває встановлений період для відновлення.
        ///
        /// (Creates a user deletion record in the Deleted state
        /// and starts the configured restoration period.)
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача.</param>
        /// <param name="login">Знімок логіна користувача.</param>
        /// <param name="now">Час створення запису.</param>
        /// <param name="reason">Нормалізована причина видалення.</param>
        /// <returns>Новий запис про видалення користувача.</returns>
        private UserDeletion(
            UserDeletionId userDeletionId, 
            UserId userId, 
            Login login,
            DateTimeOffset now, 
            DeletionReason reason)
            : base(userDeletionId, now)
        {
            UserId = userId;
            Login = login;
            Reason = reason;
            RestoreUntil = now.AddDays(RESTORATION_PERIOD_IN_DAYS);
            RestoredAt = null;
            Status = DeletionStatus.Deleted;
            UpdatedAt = null;
        }

        internal static UserDeletion Create(
            UserId userId, 
            Login login, 
            DateTimeOffset now, 
            DeletionReason reason)
        {
            DomainGuard.AgainstNull<UserDeletion>(
                OperationType.Create,
                (userId, nameof(userId)),
                (login, nameof(login)),
                (reason, nameof(reason))
            );

            return new UserDeletion(
                UserDeletionId.Create(),
                userId,
                login,
                now,
                reason
            );
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює сутність зі збережених даних і перевіряє
        /// консистентність її життєвого циклу.
        ///
        /// (Restores the entity from persisted data and validates
        /// the consistency of its lifecycle.)
        /// </summary>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язкові дані відсутні або статус не підтримується.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо часові значення та статус утворюють неможливий стан.
        /// </exception>
        private UserDeletion(
            UserDeletionId userDeletionId,
            UserId userId,
            Login login,
            DeletionReason reason,
            DateTimeOffset createdAt,
            DateTimeOffset restoreUntil,
            DateTimeOffset? restoredAt,
            DeletionStatus deletionStatus,
            DateTimeOffset? updatedAt)
            : base(userDeletionId, createdAt)
        {
            UserId = userId;
            Login = login;
            Reason = reason;
            RestoreUntil = restoreUntil;
            RestoredAt = restoredAt;
            Status = deletionStatus;
            UpdatedAt = updatedAt;
        }

        public static UserDeletion Restore(
            UserDeletionId userDeletionId,
            UserId userId,
            Login login,
            DeletionReason reason,
            DateTimeOffset createdAt,
            DateTimeOffset restoreUntil,
            DateTimeOffset? restoredAt,
            DeletionStatus deletionStatus,
            DateTimeOffset? updatedAt)
        {
            DomainGuard.AgainstNull<UserDeletion>(
                OperationType.Restore,
                (userDeletionId, nameof(userDeletionId)),
                (userId, nameof(userId)),
                (login, nameof(login)),
                (reason, nameof(reason))
            );

            ValidateState(
                OperationType.Restore,
                deletionStatus,
                updatedAt,
                createdAt,
                restoreUntil,
                restoredAt
            );

            return new UserDeletion(
                userDeletionId,
                userId,
                login,
                reason,
                createdAt,
                restoreUntil,
                restoredAt,
                deletionStatus,
                updatedAt
            );
        }
        #endregion

        #region Behavior
        /// <summary>
        /// Намагається відновити користувача протягом дозволеного періоду.
        ///
        /// (Attempts to restore the user within the allowed restoration period.)
        /// </summary>
        /// <param name="now">Час виконання операції.</param>
        /// <returns>
        /// Успішний результат після переходу в Restored або очікувану помилку,
        /// якщо користувача вже відновлено чи період відновлення завершився.
        /// </returns>
        /// /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує часу створення сутності.
        /// </exception>
        internal Result Undelete(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<UserDeletion>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            if (Status == DeletionStatus.Restored)
                return Result.Failure(UserDeletionErrors.AlreadyRestored<UserDeletion>());

            if (Status == DeletionStatus.PermanentlyDeleted || now > RestoreUntil)
                return Result.Failure(UserDeletionErrors.RestorationPeriodExpired<UserDeletion>());

            Status = DeletionStatus.Restored;
            RestoredAt = now;
            UpdatedAt = now;

            return Result.Success();
        }

        // TODO: Separate the expiration check from the state transition.
        // Repeated processing of PermanentlyDeleted must be idempotent.
        internal bool MarkAsExpired(DateTimeOffset now)
        {
            if (Status == DeletionStatus.Restored)
                return false;

            if (now <= RestoreUntil)
                return false;

            Status = DeletionStatus.PermanentlyDeleted;
            UpdatedAt = now;

            return true;
        }
        #endregion

        #region Validation
        private static void ValidateState(
            OperationType operationType,
            DeletionStatus deletionStatus,
            DateTimeOffset? updatedAt,
            DateTimeOffset createdAt,
            DateTimeOffset restoreUntil,
            DateTimeOffset? restoredAt)
        {
            DomainGuard.AgainstUndefinedEnum<UserDeletion>(
                operationType,
                (deletionStatus, nameof(deletionStatus))
            );
            
            if (restoreUntil <= createdAt)
            {
                throw DomainInvariantViolationException.BrokenState<UserDeletion>(
                    "RestoreUntil must be later than CreatedAt.",
                    new Dictionary<string, object?>
                    {
                        ["CreatedAt"] = createdAt,
                        ["RestoreUntil"] = restoreUntil
                    }
                );
            }
                
            if (updatedAt is not null && updatedAt < createdAt)
            {
                throw DomainInvariantViolationException.BrokenState<UserDeletion>(
                    $"Update time '{updatedAt}' cannot be less than creation time '{createdAt}'!",
                    new Dictionary<string, object?>
                    {
                        ["CreatedAt"] = createdAt,
                        ["UpdatedAt"] = updatedAt
                    }
                );
            }
                

            if (restoredAt is not null && restoredAt < createdAt)
            {
                throw DomainInvariantViolationException.BrokenState<UserDeletion>(
                   $"The recovery time '{restoredAt}' cannot be less than creation time '{createdAt}'!",
                   new Dictionary<string, object?>
                   {
                       ["RestoredAt"] = restoredAt,
                       ["CreatedAt"] = createdAt
                   }
               );
            }
            
            bool isValidState = deletionStatus switch
            {
                DeletionStatus.Deleted =>
                    updatedAt is null
                    && restoredAt is null,

                DeletionStatus.Restored =>
                    updatedAt is not null
                    && restoredAt is not null
                    && restoredAt.Value <= restoreUntil
                    && updatedAt.Value == restoredAt.Value,

                DeletionStatus.PermanentlyDeleted =>
                    updatedAt is not null
                    && restoredAt is null
                    && updatedAt.Value > restoreUntil,

                _ => false
            };

            if (!isValidState)
            {
                throw DomainInvariantViolationException.BrokenState<UserDeletion>(
                    $"Inconsistent state: fields '{nameof(deletionStatus)}' and '{nameof(updatedAt)}' " +
                    $"and '{nameof(restoredAt)}' are not consistent!",
                    new Dictionary<string, object?>
                    {
                        ["DeletionStatus"] = deletionStatus,
                        ["UpdatedAt"] = updatedAt,
                        ["RestoredAt"] = restoredAt,
                        ["CreatedAt"] = createdAt,
                        ["RestoreUntil"] = restoreUntil,
                    }
                );
            }
        }
        #endregion
    }
}
