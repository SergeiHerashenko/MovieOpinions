using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.Entities.UsersDeletion.Errors;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Entities.UsersDeletion
{
    /// <summary>
    /// Дочірня сутність агрегату User, яка представляє запис
    /// про видалення користувача та керує життєвим циклом цього запису.
    /// Підтримує переходи зі стану Deleted у Restored
    /// або PermanentlyDeleted.
    ///
    /// (Child entity of the User aggregate representing a user deletion record
    /// and managing its lifecycle. Supports transitions from Deleted
    /// to Restored or PermanentlyDeleted.)
    /// </summary>
    public sealed class UserDeletion : Entity<UserDeletionId>, IUserOwned
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
        /// Момент завершення періоду відновлення.
        /// Користувача можна відновити лише до настання цього моменту.
        ///
        /// (The moment when the restoration period ends.
        /// The user can be restored only before this moment.)
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
        /// Ініціалізує новий запис про видалення у стані Deleted
        /// та встановлює період відновлення.
        ///
        /// (Initializes a new deletion record in the Deleted state
        /// and establishes its restoration period.)
        /// </summary>
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

        /// <summary>
        /// Створює новий запис про видалення користувача у стані Deleted,
        /// генерує його ідентифікатор та встановлює період відновлення.
        ///
        /// (Creates a new user deletion record in the Deleted state,
        /// generates its identifier, and establishes the restoration period.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача.
        ///
        /// (The user identifier.)
        /// </param>
        /// <param name="login">
        /// Знімок логіна користувача на момент видалення.
        ///
        /// (A snapshot of the user login at the time of deletion.)
        /// </param>
        /// <param name="now">
        /// Час створення запису про видалення.
        ///
        /// (The deletion record creation time.)
        /// </param>
        /// <param name="reason">
        /// Нормалізована причина видалення.
        ///
        /// (The normalized deletion reason.)
        /// </param>
        /// <returns>
        /// Створений запис про видалення користувача.
        ///
        /// (The newly created user deletion record.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язковий доменний об’єкт дорівнює null.
        ///
        /// (Thrown when a required domain object is null.)
        /// </exception>
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
        /// Ініціалізує запис про видалення зі збережених значень,
        /// попередньо перевірених фабрикою Restore.
        ///
        /// (Initializes a deletion record from persisted values
        /// previously validated by the Restore factory.)
        /// </summary>
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

        /// <summary>
        /// Перевіряє збережений стан і відновлює
        /// запис про видалення користувача.
        ///
        /// (Validates the persisted state and restores
        /// the user deletion record.)
        /// </summary>
        /// <param name="userDeletionId">
        /// Збережений ідентифікатор запису про видалення.
        ///
        /// (The persisted deletion record identifier.)
        /// </param>
        /// <param name="userId">
        /// Ідентифікатор користувача.
        ///
        /// (The user identifier.)
        /// </param>
        /// <param name="login">
        /// Знімок логіна користувача на момент видалення.
        ///
        /// (A snapshot of the user login at the time of deletion.)
        /// </param>
        /// <param name="reason">
        /// Збережена причина видалення.
        ///
        /// (The persisted deletion reason.)
        /// </param>
        /// <param name="createdAt">
        /// Час створення запису про видалення.
        ///
        /// (The deletion record creation time.)
        /// </param>
        /// <param name="restoreUntil">
        /// Момент завершення періоду відновлення.
        ///
        /// (The moment when the restoration period ends.)
        /// </param>
        /// <param name="restoredAt">
        /// Час відновлення або null.
        ///
        /// (The restoration time, or null.)
        /// </param>
        /// <param name="deletionStatus">
        /// Збережений стан життєвого циклу видалення.
        ///
        /// (The persisted deletion lifecycle state.)
        /// </param>
        /// <param name="updatedAt">
        /// Час останнього переходу стану або null.
        ///
        /// (The time of the most recent state transition, or null.)
        /// </param>
        /// <returns>
        /// Відновлений запис про видалення користувача.
        ///
        /// (The restored user deletion record.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язкові доменні об’єкти відсутні
        /// або статус не підтримується.
        ///
        /// (Thrown when required domain objects are missing
        /// or the status is unsupported.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо часові значення або їх відповідність статусу
        /// утворюють неконсистентний стан.
        ///
        /// (Thrown when timestamp values or their relationship
        /// with the status form an inconsistent state.)
        /// </exception>
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
        /// Намагається перевести запис про видалення у стан Restored
        /// протягом дозволеного періоду відновлення.
        ///
        /// Після успішного переходу встановлює RestoredAt і UpdatedAt
        /// відповідно до часу операції.
        ///
        /// (Attempts to transition the deletion record to Restored
        /// within the permitted restoration period.
        ///
        /// After a successful transition, sets RestoredAt and UpdatedAt
        /// to the operation time.)
        /// </summary>
        /// <param name="now">
        /// Час виконання операції відновлення.
        ///
        /// (The restoration operation time.)
        /// </param>
        /// <returns>
        /// Успішний результат переходу в Restored або помилка,
        /// якщо обліковий запис уже відновлено, остаточно видалено
        /// чи період відновлення завершився.
        ///
        /// (A successful result for the transition to Restored, or an error
        /// when the account has already been restored, has been permanently deleted,
        /// or its restoration period has expired.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує часу створення запису.
        ///
        /// (Thrown when the operation time precedes the record creation time.)
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

            if (Status == DeletionStatus.PermanentlyDeleted)
                return Result.Failure(UserDeletionErrors.RestorationPeriodExpired<UserDeletion>());

            if (now >= RestoreUntil)
                return Result.Failure(UserDeletionErrors.RestorationPeriodExpired<UserDeletion>());

            Status = DeletionStatus.Restored;
            RestoredAt = now;
            UpdatedAt = now;

            return Result.Success();
        }

        /// <summary>
        /// Намагається перевести запис зі стану Deleted у PermanentlyDeleted
        /// після завершення періоду відновлення.
        ///
        /// Після успішного переходу записує його фактичний час в UpdatedAt.
        ///
        /// (Attempts to transition the record from Deleted to PermanentlyDeleted
        /// after the restoration period has ended.
        ///
        /// After a successful transition, records its actual time in UpdatedAt.)
        /// </summary>
        /// <param name="now">
        /// Час перевірки та можливого переходу стану.
        ///
        /// (The time used for validation and a possible state transition.)
        /// </param>
        /// <returns>
        /// true, якщо стан змінено на PermanentlyDeleted;
        /// інакше false, якщо запис уже має кінцевий статус
        /// або період відновлення ще не завершився.
        ///
        /// (true when the state changes to PermanentlyDeleted;
        /// otherwise, false when the record already has a terminal status
        /// or the restoration period has not yet ended.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує часу створення запису
        /// або поточний статус не підтримується.
        ///
        /// (Thrown when the operation time precedes the record creation time
        /// or the current status is unsupported.)
        /// </exception>
        internal bool TryMarkAsPermanentlyDeleted(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<UserDeletion>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            DomainGuard.AgainstUndefinedEnum<UserDeletion>(
                OperationType.Update,
                (Status, nameof(Status))
            );

            if (Status == DeletionStatus.Restored)
                return false;

            if (Status == DeletionStatus.PermanentlyDeleted)
                return false;

            if (now < RestoreUntil)
                return false;

            Status = DeletionStatus.PermanentlyDeleted;
            UpdatedAt = now;

            return true;
        }
        #endregion

        #region Validation
        /// <summary>
        /// Перевіряє консистентність збереженого стану запису про видалення.
        ///
        /// Контролює допустимість статусу, часову послідовність
        /// та узгодженість часових полів зі станом життєвого циклу.
        ///
        /// (Validates the consistency of a persisted deletion record state.
        ///
        /// Checks the status value, chronological order,
        /// and consistency between lifecycle timestamps and the status.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, у межах якої перевіряється стан.
        ///
        /// (The operation during which the state is validated.)
        /// </param>
        /// <param name="deletionStatus">
        /// Стан життєвого циклу видалення.
        ///
        /// (The deletion lifecycle state.)
        /// </param>
        /// <param name="updatedAt">
        /// Час останнього переходу стану або null.
        ///
        /// (The time of the most recent state transition, or null.)
        /// </param>
        /// <param name="createdAt">
        /// Час створення запису про видалення.
        ///
        /// (The deletion record creation time.)
        /// </param>
        /// <param name="restoreUntil">
        /// Момент завершення періоду відновлення.
        ///
        /// (The moment when the restoration period ends.)
        /// </param>
        /// <param name="restoredAt">
        /// Час відновлення або null.
        ///
        /// (The restoration time, or null.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо статус не підтримується.
        ///
        /// (Thrown when the status is unsupported.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо часові значення або їх відповідність статусу
        /// утворюють неконсистентний стан.
        ///
        /// (Thrown when timestamp values or their relationship
        /// with the status form an inconsistent state.)
        /// </exception>
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
                    },
                    operationType
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
                    },
                    operationType
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
                   },
                   operationType
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
                    && restoredAt.Value < restoreUntil
                    && updatedAt.Value == restoredAt.Value,

                DeletionStatus.PermanentlyDeleted =>
                    updatedAt is not null
                    && restoredAt is null
                    && updatedAt.Value >= restoreUntil,

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
                    },
                    operationType
                );
            }
        }
        #endregion
    }
}
