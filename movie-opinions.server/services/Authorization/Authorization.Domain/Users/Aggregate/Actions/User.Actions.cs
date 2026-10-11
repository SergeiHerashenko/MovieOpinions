using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Action;
using Authorization.Domain.Users.DomainEvents.Actions;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Errors;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        #region RequestLoginChange
        /// <summary>
        /// Створює запит на зміну логіна користувача.
        ///
        /// Перевіряє доступність операції та відхиляє запит,
        /// якщо переданий логін збігається з поточним.
        /// Успішний запит створює відкладену дію, яка потребує
        /// подальшого підтвердження.
        ///
        /// Поточний логін користувача цим методом не змінюється.
        ///
        /// (Creates a request to change the user's login.
        ///
        /// Validates operation access and rejects the request when
        /// the supplied login matches the current login.
        /// A successful request creates a pending action
        /// that requires subsequent confirmation.
        ///
        /// This method does not change the user's current login.)
        /// </summary>
        /// <param name="newLogin">Новий логін, який користувач хоче встановити.</param>
        /// <param name="now">Час створення запиту.</param>
        /// <returns>
        /// Створена відкладена дія або помилка, якщо запит не може бути прийнятий.
        /// </returns>
        public Result<UserPendingAction> RequestLoginChange(
            Login newLogin,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (newLogin, nameof(newLogin))
            );

            var resultAccess = ValidateAccessForPendingActionRequest(now);

            if (resultAccess.IsFailure)
                return Result<UserPendingAction>.Failure(resultAccess.Errors);

            if (newLogin == Login)
                return Result<UserPendingAction>.Failure(UserErrors.NoUpdateNeeded<User>());

            var userAction = UserAction.From(newLogin);

            var requestResult = RequestPendingAction(
                userAction,
                now
            );

            if (requestResult.IsFailure)
                return Result<UserPendingAction>.Failure(requestResult.Errors);

            var pendingAction = requestResult.Value;

            AddDomainEvent(new UserLoginChangeRequestedEvent(
                Id,
                pendingAction.Id,
                Login,
                newLogin,
                pendingAction.ConfirmationToken,
                pendingAction.ExpiresAt,
                now)
            );

            return Result<UserPendingAction>.Success(pendingAction);
        }
        #endregion

        #region RequestPasswordChange
        /// <summary>
        /// Створює запит на зміну пароля користувача.
        ///
        /// Перевіряє доступність операції, відповідність переданого
        /// відкритого пароля його хешу та відмінність нового пароля
        /// від поточного.
        ///
        /// Успішний запит створює відкладену дію, яка потребує
        /// подальшого підтвердження. Поточний пароль цим методом
        /// не змінюється.
        ///
        /// (Creates a request to change the user's password.
        ///
        /// Validates operation access, verifies that the supplied plain
        /// password corresponds to its hash, and ensures that the new
        /// password differs from the current password.
        ///
        /// A successful request creates a pending action that requires
        /// subsequent confirmation. This method does not change
        /// the current password.)
        /// </summary>
        /// <param name="plainNewPassword">
        /// Новий пароль у відкритому вигляді, який використовується для перевірки.
        /// </param>
        /// <param name="newPassword">Хеш нового пароля.</param>
        /// <param name="verifier">
        /// Функція перевірки відповідності відкритого пароля його хешу.
        /// </param>
        /// <param name="now">Час створення запиту.</param>
        /// <returns>
        /// Створена відкладена дія або помилка, якщо запит не може бути прийнятий.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Переданий хеш не відповідає відкритому новому паролю.
        /// </exception>
        public Result<UserPendingAction> RequestPasswordChange(
            PlainPassword plainNewPassword,
            Password newPassword,
            Func<PlainPassword, PasswordHash, bool> verifier,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (plainNewPassword, nameof(plainNewPassword)),
                (newPassword, nameof(newPassword)),
                (verifier, nameof(verifier))
            );

            var resultAccess = ValidateAccessForPendingActionRequest(now);

            if (resultAccess.IsFailure)
                return Result<UserPendingAction>.Failure(resultAccess.Errors);

            var newPasswordMatchesPlainPassword = newPassword.Matches(
                plainNewPassword,
                verifier
            );

            if (!newPasswordMatchesPlainPassword)
            {
                throw DomainInvalidOperationException.PreconditionFailed<User>(
                    nameof(RequestPasswordChange),
                    "The supplied password hash must correspond to the supplied plain password.",
                    OperationType.Update,
                    context: new Dictionary<string, object>
                    {
                        ["UserId"] = Id.Value
                    }
                );
            }

            var matchesCurrentPassword = Password.Matches(
                plainNewPassword,
                verifier
            );

            if (matchesCurrentPassword)
                return Result<UserPendingAction>.Failure(UserErrors.NoUpdateNeeded<User>());

            var userAction = UserAction.From(newPassword);

            var requestResult = RequestPendingAction(
                userAction,
                now
            );

            if (requestResult.IsFailure)
                return Result<UserPendingAction>.Failure(requestResult.Errors);

            var pendingAction = requestResult.Value;

            AddDomainEvent(new UserPasswordChangeRequestedEvent(
                Id,
                pendingAction.Id,
                Login,
                pendingAction.ConfirmationToken,
                pendingAction.ExpiresAt,
                now)
            );

            return Result<UserPendingAction>.Success(pendingAction);
        }
        #endregion

        #region RequestAccountDeletion
        /// <summary>
        /// Створює запит на видалення облікового запису користувача.
        ///
        /// Перевіряє доступність операції та допустимість причини
        /// видалення, після чого створює відкладену дію,
        /// яка потребує подальшого підтвердження.
        ///
        /// Користувач цим методом не видаляється.
        ///
        /// (Creates a request to delete the user's account.
        ///
        /// Validates operation access and the supplied deletion reason,
        /// then creates a pending action that requires subsequent
        /// confirmation.
        ///
        /// This method does not delete the user.)
        /// </summary>
        /// <param name="reason">Необов’язкова причина видалення облікового запису.</param>
        /// <param name="now">Час створення запиту.</param>
        /// <returns>
        /// Створена відкладена дія або помилка, якщо запит не може бути прийнятий.
        /// </returns>
        public Result<UserPendingAction> RequestAccountDeletion(
            string? reason,
            DateTimeOffset now)
        {
            var resultAccess = ValidateAccessForPendingActionRequest(now);

            if (resultAccess.IsFailure)
                return Result<UserPendingAction>.Failure(resultAccess.Errors);

            var deletionReason = DeletionReason.Create(reason);

            if (deletionReason.IsFailure)
                return Result<UserPendingAction>.Failure(deletionReason.Errors);

            var userAction = UserAction.From(deletionReason.Value);

            var requestResult = RequestPendingAction(
                userAction,
                now
            );

            if (requestResult.IsFailure)
                return Result<UserPendingAction>.Failure(requestResult.Errors);

            var pendingAction = requestResult.Value;

            AddDomainEvent(new UserAccountDeletionRequestedEvent(
                Id,
                pendingAction.Id,
                Login,
                pendingAction.ConfirmationToken,
                pendingAction.ExpiresAt,
                now)
            );

            return Result<UserPendingAction>.Success(pendingAction);
        }
        #endregion

        #region Pending Action Request Processing
        /// <summary>
        /// Перевіряє часову послідовність запиту та доступність
        /// користувача для створення відкладеної дії.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Validates the request timestamp and verifies that the user
        /// is allowed to create a pending action.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="now">Час виконання запиту.</param>
        /// <returns>
        /// Успіх, якщо операція дозволена; інакше — помилка доступу.
        /// </returns>
        private Result ValidateAccessForPendingActionRequest(DateTimeOffset now)
        {
            EnsureValidOperationTimestamp(now);

            var access = ProvideAccess(now);

            if (access.IsFailure)
                return Result.Failure(access.Errors);

            return Result.Success();
        }

        /// <summary>
        /// Виконує спільний процес створення відкладеної дії.
        ///
        /// Готує відповідний план, застосовує його, оновлює час
        /// зміни користувача та записує зміни агрегату.
        /// Якщо підготовка плану завершується помилкою,
        /// стан агрегату не змінюється.
        ///
        /// Конкретну доменну подію записує публічний метод,
        /// який ініціював запит.
        ///
        /// (Executes the shared pending-action creation process.
        ///
        /// Prepares and applies the appropriate plan, updates the user's
        /// modification timestamp, and records aggregate changes.
        /// If plan preparation fails, aggregate state remains unchanged.
        ///
        /// The concrete domain event is recorded by the public method
        /// that initiated the request.)
        /// </summary>
        /// <param name="userAction">Дані запитаної дії.</param>
        /// <param name="now">Час створення відкладеної дії.</param>
        /// <returns>
        /// Створена відкладена дія або помилка підготовки плану.
        /// </returns>
        private Result<UserPendingAction> RequestPendingAction(
            UserAction userAction,
            DateTimeOffset now)
        {
            var planResult = PreparePendingActionRequestPlan(
                userAction,
                now
            );

            if (planResult.IsFailure)
                return Result<UserPendingAction>.Failure(planResult.Errors);

            var plan = planResult.Value;

            plan.Apply(
                this,
                now
            );

            UpdatedAt = now;

            plan.RecordAggregateChanges(
                this,
                now
            );

            return Result<UserPendingAction>.Success(plan.Action);
        }

        /// <summary>
        /// Визначає та готує план створення відкладеної дії.
        ///
        /// Якщо поточної дії немає, створює план звичайного додавання.
        /// Якщо поточна дія ще активна, повертає помилку.
        /// Якщо строк активної дії завершився, перевіряє можливість
        /// її завершення та створює план заміни.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Determines and prepares a pending-action request plan.
        ///
        /// When no current action exists, creates a standard creation plan.
        /// When the current action is still active, returns a failure.
        /// When the active action has expired, validates its expiration
        /// and creates a replacement plan.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="userAction">Дані нової відкладеної дії.</param>
        /// <param name="now">Час, відносно якого перевіряється поточна дія.</param>
        /// <returns>
        /// Підготовлений план або помилка, якщо поточна дія ще активна.
        /// </returns>
        private Result<PendingActionRequestPlan> PreparePendingActionRequestPlan(
            UserAction userAction,
            DateTimeOffset now)
        {
            var currentAction = _action;

            if (currentAction is null)
            {
                var newAction = UserPendingAction.Create(
                    Id,
                    userAction,
                    now
                );

                return Result<PendingActionRequestPlan>.Success(
                    new CreatePendingActionPlan(newAction)
                );
            }

            if (currentAction.IsActive(now))
                return Result<PendingActionRequestPlan>.Failure(UserErrors.ActionAlreadyExists<User>());

            currentAction.ValidateCanExpire(now);

            var replacementAction = UserPendingAction.Create(
                Id,
                userAction,
                now
            );

            return Result<PendingActionRequestPlan>.Success(
                new ReplaceExpiredPendingActionPlan(
                    replacementAction,
                    currentAction
                )
            );
        }
        #endregion

        #region Pending Action Request Plans
        /// <summary>
        /// Базовий план створення або заміни відкладеної дії користувача.
        ///
        /// Зберігає нову дію та визначає окремі етапи
        /// застосування доменного стану і запису змін агрегату.
        ///
        /// (Base plan for creating or replacing a user's pending action.
        ///
        /// Stores the new action and defines separate stages for applying
        /// domain state and recording aggregate changes.)
        /// </summary>
        private abstract class PendingActionRequestPlan
        {
            public UserPendingAction Action { get; }

            protected PendingActionRequestPlan(UserPendingAction action)
            {
                Action = action;
            }

            /// <summary>
            /// Застосовує підготовлені планом зміни доменного стану.
            ///
            /// Метод не записує зміни агрегату або доменні події.
            ///
            /// (Applies the domain-state changes prepared by the plan.
            ///
            /// The method does not record aggregate changes or domain events.)
            /// </summary>
            /// <param name="user">Користувач, стан якого необхідно змінити.</param>
            /// <param name="now">Час застосування плану.</param>
            public abstract void Apply(
                User user,
                DateTimeOffset now);

            /// <summary>
            /// Записує зміни агрегату, що відповідають
            /// уже застосованим переходам стану.
            ///
            /// Метод не виконує повторних мутацій.
            ///
            /// (Records aggregate changes corresponding
            /// to the already applied state transitions.
            ///
            /// The method does not repeat state mutations.)
            /// </summary>
            /// <param name="user">Користувач, до якого додаються зміни.</param>
            /// <param name="now">Час запису змін.</param>
            public abstract void RecordAggregateChanges(
                User user,
                DateTimeOffset now);
        }

        /// <summary>
        /// План створення відкладеної дії, коли користувач
        /// не має поточної дії.
        ///
        /// (Plan for creating a pending action when the user
        /// has no current action.)
        /// </summary>
        private sealed class CreatePendingActionPlan : PendingActionRequestPlan
        {
            public CreatePendingActionPlan(UserPendingAction action)
                : base(action) { }

            public override void Apply(
                User user,
                DateTimeOffset now)
            {
                user.SetPendingAction(Action);
            }

            public override void RecordAggregateChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddAggregateChange(new UserPendingActionCreated(
                    Action,
                    now)
                );
            }
        }

        /// <summary>
        /// План завершення попередньої відкладеної дії за часом
        /// та її заміни новою дією.
        ///
        /// (Plan for expiring the previous pending action
        /// and replacing it with a new action.)
        /// </summary>  
        private sealed class ReplaceExpiredPendingActionPlan : PendingActionRequestPlan
        {
            public UserPendingAction ExpiredAction { get; }

            public ReplaceExpiredPendingActionPlan(
                UserPendingAction newAction,
                UserPendingAction expiredAction)
                : base(newAction)
            {
                ExpiredAction = expiredAction;
            }

            public override void Apply(
                User user,
                DateTimeOffset now)
            {
                ExpiredAction.MarkAsExpired(now);

                user.SetPendingAction(Action);
            }

            public override void RecordAggregateChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddAggregateChange(new UserPendingActionUpdated(
                    ExpiredAction,
                    now)
                );

                user.AddAggregateChange(new UserPendingActionCreated(
                    Action,
                    now)
                );
            }
        }
        #endregion
    }
}
