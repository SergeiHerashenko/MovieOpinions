using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.Errors;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Entities.UsersPendingAction
{
    /// <summary>
    /// Дочірня сутність агрегату User, яка представляє відкладену
    /// дію користувача та керує її життєвим циклом.
    /// Зберігає дані дії, токен підтвердження, строк дії та поточний статус.
    ///
    /// (Child entity of the User aggregate representing a pending user action
    /// and managing its lifecycle. Stores the action data, confirmation token,
    /// expiration time, and current status.)
    /// </summary>
    public sealed class UserPendingAction : Entity<UserPendingActionId>
    {
        private const int EXPIRATION_TIME_IN_MINUTES = 30;

        #region Properties
        /// <summary>
        /// Ідентифікатор користувача, якому належить відкладена дія.
        ///
        /// (Identifier of the user that owns the pending action.)
        /// </summary>
        public UserId UserId { get; }

        /// <summary>
        /// Токен, який використовується для підтвердження або скасування дії.
        ///
        /// (Token used to confirm or cancel the action.)
        /// </summary>
        public ConfirmationFlowToken ConfirmationToken { get; }

        /// <summary>
        /// Дані операції, яку потрібно виконати після підтвердження.
        ///
        /// (Data of the operation to be performed after confirmation.)
        /// </summary>
        public UserAction UserAction { get; }

        /// <summary>
        /// Дата й час завершення строку дії.
        ///
        /// (Date and time when the action expires.)
        /// </summary>
        public DateTimeOffset ExpiresAt { get; }

        /// <summary>
        /// Час підтвердження дії або null, якщо її не підтверджено.
        ///
        /// (Time when the action was confirmed, or null if it was not confirmed.)
        /// </summary>
        public DateTimeOffset? ConfirmationTime { get; private set; }

        /// <summary>
        /// Час переходу дії у стан Expired або null.
        ///
        /// (Time when the action transitioned to Expired, or null.)
        /// </summary>
        public DateTimeOffset? ExpiredAt { get; private set; }

        /// <summary>
        /// Час скасування дії або null, якщо її не скасовано.
        ///
        /// (Time when the action was cancelled, or null if it was not cancelled.)
        /// </summary>
        public DateTimeOffset? CancelledTime { get; private set; }

        /// <summary>
        /// Поточний стан життєвого циклу відкладеної дії.
        ///
        /// (Current lifecycle status of the pending action.)
        /// </summary>
        public ActionStatus Status { get; private set; }
        #endregion

        #region Creation
        /// <summary>
        /// Створює нову відкладену дію у стані Active,
        /// генерує її ID і токен підтвердження та встановлює строк дії.
        ///
        /// (Creates a new pending action in the Active state,
        /// generates its ID and confirmation token, and sets its expiration time.)
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача.</param>
        /// <param name="userAction">Дані запланованої дії.</param>
        /// <param name="now">Час створення сутності.</param>
        /// <returns>Створена відкладена дія.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язковий доменний об’єкт дорівнює null.
        /// </exception>
        private UserPendingAction(
            UserPendingActionId userPendingActionId, 
            UserId userId, 
            ConfirmationFlowToken confirmationFlowToken, 
            UserAction userAction,
            DateTimeOffset now)
            : base(userPendingActionId, now)
        {
            UserId = userId;
            ConfirmationToken = confirmationFlowToken;
            UserAction = userAction;
            ExpiresAt = now.Add(TimeSpan.FromMinutes(EXPIRATION_TIME_IN_MINUTES));
            ConfirmationTime = null;
            ExpiredAt = null;
            CancelledTime = null;
            Status = ActionStatus.Active;
        }

        internal static UserPendingAction Create(
            UserId userId,
            UserAction userAction,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<UserPendingAction>(
                OperationType.Create,
                (userId, nameof(userId)),
                (userAction, nameof(userAction))
            );

            var pendingAction = new UserPendingAction(
                UserPendingActionId.Create(),
                userId,
                ConfirmationFlowToken.Create(),
                userAction,
                now
            );

            return pendingAction;
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює відкладену дію зі збереженого стану
        /// та перевіряє консистентність її життєвого циклу.
        ///
        /// (Restores a pending action from persisted state
        /// and validates the consistency of its lifecycle.)
        /// </summary>
        /// <param name="userPendingActionId">Ідентифікатор сутності.</param>
        /// <param name="userId">Ідентифікатор користувача.</param>
        /// <param name="confirmationToken">Збережений токен підтвердження.</param>
        /// <param name="userAction">Збережені дані дії.</param>
        /// <param name="expiresAt">Час завершення строку дії.</param>
        /// <param name="confirmationTime">Час підтвердження або null.</param>
        /// <param name="expiredAt">Час завершення строку дії або null.</param>
        /// <param name="cancelledTime">Час скасування або null.</param>
        /// <param name="actionStatus">Збережений статус дії.</param>
        /// <param name="createdAt">Час створення сутності.</param>
        /// <returns>Відновлена відкладена дія.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язкові дані відсутні або статус не підтримується.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо статус і часові значення утворюють неможливий стан.
        /// </exception>
        private UserPendingAction(
            UserPendingActionId userPendingActionId,
            UserId userId,
            ConfirmationFlowToken confirmationFlowToken,
            UserAction userAction,
            DateTimeOffset expiresAt,
            DateTimeOffset? confirmationTime,
            DateTimeOffset? expiredAt,
            DateTimeOffset? cancelledTime,
            ActionStatus actionStatus,
            DateTimeOffset createdAt)
            : base(userPendingActionId, createdAt)
        {
            UserId = userId;
            ConfirmationToken = confirmationFlowToken;
            UserAction = userAction;
            ExpiresAt = expiresAt;
            ConfirmationTime = confirmationTime;
            ExpiredAt = expiredAt;
            CancelledTime = cancelledTime;
            Status = actionStatus;
        }

        public static UserPendingAction Restore(
            UserPendingActionId userPendingActionId,
            UserId userId,
            ConfirmationFlowToken confirmationToken,
            UserAction userAction,
            DateTimeOffset expiresAt,
            DateTimeOffset? confirmationTime,
            DateTimeOffset? expiredAt,
            DateTimeOffset? cancelledTime,
            ActionStatus actionStatus,
            DateTimeOffset createdAt)
        {
            DomainGuard.AgainstNull<UserPendingAction>(
                OperationType.Restore,
                (userPendingActionId, nameof(userPendingActionId)),
                (userId, nameof(userId)),
                (confirmationToken, nameof(confirmationToken)),
                (userAction, nameof(userAction))
            );

            ValidateState(
                userPendingActionId,
                actionStatus,
                expiresAt,
                confirmationTime,
                expiredAt,
                createdAt,
                cancelledTime
            );

            return new UserPendingAction(
                userPendingActionId,
                userId,
                confirmationToken,
                userAction,
                expiresAt,
                confirmationTime,
                expiredAt,
                cancelledTime,
                actionStatus,
                createdAt
            );
        }
        #endregion

        #region Behavior
        /// <summary>
        /// Намагається підтвердити активну дію за допомогою токена підтвердження.
        ///
        /// (Attempts to confirm an active action using its confirmation token.)
        /// </summary>
        /// <returns>
        /// Успішний результат після переходу в Confirmed або очікувану помилку,
        /// якщо токен не збігається, дія неактивна чи строк її дії завершився.
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо токен дорівнює null або час операції передує створенню сутності.
        /// </exception>
        internal Result ConfirmAction(
            ConfirmationFlowToken confirmationToken,
            DateTimeOffset now)
        {
            return ChangeActionStatus(
                confirmationToken,
                now,
                ActionStatus.Confirmed,
                () =>
                {
                    Status = ActionStatus.Confirmed;
                    ConfirmationTime = now;
                }
            );
        }

        /// <summary>
        /// Намагається скасувати активну дію за допомогою токена підтвердження.
        ///
        /// (Attempts to cancel an active action using its confirmation token.)
        /// </summary>
        /// <returns>
        /// Успішний результат після переходу в Cancelled або очікувану помилку,
        /// якщо токен не збігається, дія неактивна чи строк її дії завершився.
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо токен дорівнює null або час операції передує створенню сутності.
        /// </exception>
        internal Result CancelAction(
            ConfirmationFlowToken confirmationToken,
            DateTimeOffset now)
        {
            return ChangeActionStatus(
                confirmationToken,
                now,
                ActionStatus.Cancelled,
                () =>
                {
                    Status = ActionStatus.Cancelled;
                    CancelledTime = now;
                }
            );
        }

        /// <summary>
        /// Виконує спільні перевірки переходу з Active у цільовий стан
        /// та застосовує передану зміну.
        ///
        /// (Performs common checks for a transition from Active
        /// to the target status and applies the supplied change.)
        /// </summary>
        private Result ChangeActionStatus(
            ConfirmationFlowToken confirmationFlowToken,
            DateTimeOffset now,
            ActionStatus targetStatus,
            Action applyActionStatus)
        {
            DomainGuard.AgainstNull<UserPendingAction>(
                OperationType.Update,
                (confirmationFlowToken, nameof(confirmationFlowToken))
            );

            DomainGuard.AgainstEarlierThan<UserPendingAction>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            if (confirmationFlowToken != ConfirmationToken)
                return Result.Failure(PendingActionErrors.InvalidConfirmationToken<UserPendingAction>());

            if (Status != ActionStatus.Active)
            {
                return Result.Failure(
                    PendingActionErrors.InvalidStatusTransition<UserPendingAction>(
                        Status,
                        targetStatus
                    )
                );
            }

            if (ExpiresAt <= now)
            {
                Status = ActionStatus.Expired;
                ExpiredAt = now;

                return Result.Failure(PendingActionErrors.ExpiredAction<UserPendingAction>());
            }

            applyActionStatus();

            return Result.Success();
        }

        /// <summary>
        /// Переводить активну дію у стан Failed.
        ///
        /// (Transitions an active action to the Failed state.)
        /// </summary>
        /// <returns>
        /// Успішний результат після зміни статусу або очікувану помилку,
        /// якщо дія вже не є активною.
        /// </returns>
        internal Result MarkAsFailed()
        {
            if (Status != ActionStatus.Active)
            {
                return Result.Failure(
                     PendingActionErrors.InvalidStatusTransition<UserPendingAction>(
                         Status,
                         ActionStatus.Failed
                     )
                );
            }

            Status = ActionStatus.Failed;

            return Result.Success();
        }

        internal bool MarkAsExpired(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<UserPendingAction>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            if (Status != ActionStatus.Active)
                return false;

            if (now < ExpiresAt)
                return false;

            Status = ActionStatus.Expired;
            ExpiredAt = now;

            return true;
        }
        #endregion

        #region Validation
        /// <summary>
        /// Перевіряє узгодженість статусу, строку дії
        /// та часових позначок життєвого циклу.
        ///
        /// (Validates the consistency of the status, expiration time,
        /// and lifecycle timestamps.)
        /// </summary>
        private static void ValidateState(
            UserPendingActionId userPendingActionId,
            ActionStatus actionStatus,
            DateTimeOffset expiresAt,
            DateTimeOffset? confirmationTime,
            DateTimeOffset? expiredAt,
            DateTimeOffset createdAt,
            DateTimeOffset? cancelledTime)
        {
            if (!Enum.IsDefined(actionStatus))
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<UserPendingAction>(
                    nameof(actionStatus),
                    actionStatus
                );

            if (expiresAt <= createdAt)
            {
                throw DomainInvariantViolationException.BrokenState<UserPendingAction>(
                    $"The expiration time '{nameof(expiresAt)}' cannot be less than (or equal to) " +
                    $"the creation time '{nameof(createdAt)}'!",
                    new Dictionary<string, object?>
                    {
                        ["ExpiresAt"] = expiresAt,
                        ["CreatedAt"] = createdAt
                    }
                );
            }
                
            bool isValidState = (actionStatus, confirmationTime, expiredAt, cancelledTime) switch
            {
                (ActionStatus.Active, null, null, null) => true,

                (ActionStatus.Confirmed, not null, null, null) =>
                    confirmationTime.Value >= createdAt &&
                    confirmationTime.Value < expiresAt,

                (ActionStatus.Expired, null, not null, null) =>
                    expiredAt.Value >= expiresAt,

                (ActionStatus.Cancelled, null, null, not null) =>
                    cancelledTime.Value >= createdAt &&
                    cancelledTime.Value < expiresAt,

                (ActionStatus.Failed, null, null, null) => true,

                _ => false
            };

            if (!isValidState)
            {
                throw DomainInvariantViolationException.BrokenState<UserPendingAction>(
                    $"Dates conflict. ConfirmationTime: {confirmationTime?.ToString() ?? "null"}, " +
                    $"ExpiredAt: {expiredAt?.ToString() ?? "null"}",
                    new Dictionary<string, object?>
                    {
                        ["Id"] = userPendingActionId,
                        ["ActionStatus"] = actionStatus,
                        ["CreatedAt"] = createdAt,
                        ["ExpiresAt"] = expiresAt,
                        ["ConfirmationTime"] = confirmationTime,
                        ["ExpiredAt"] = expiredAt,
                        ["CancelledTime"] = cancelledTime
                    }
                );
            }
        }
        #endregion
    }
}
