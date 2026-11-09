using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Action;
using Authorization.Domain.Users.DomainEvents.Actions;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Errors;

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        #region RequestLoginChange
        /// <summary>
        /// Створює запит на зміну логіна користувача.
        ///
        /// Перевіряє часову послідовність операції та доступ користувача.
        /// Відхиляє запит, якщо новий логін збігається з поточним.
        ///
        /// Після успішної перевірки створює відкладену дію,
        /// яка потребує подальшого підтвердження, та записує
        /// доменну подію запиту зміни логіна.
        ///
        /// Поточний логін користувача цим методом не змінюється.
        ///
        /// (Creates a request to change the user's login.
        ///
        /// Validates chronological consistency and user access.
        /// Rejects the request when the new login matches the current login.
        ///
        /// After successful validation, creates a pending action
        /// that requires subsequent confirmation and records
        /// a login-change request domain event.
        ///
        /// This method does not change the user's current login.)
        /// </summary>
        /// <param name="newLogin">
        /// Новий логін, який користувач хоче встановити.
        ///
        /// (New login the user wants to set.)
        /// </param>
        /// <param name="now">
        /// Час перевірок і створення запиту.
        ///
        /// (Time used for validation and request creation.)
        /// </param>
        /// <returns>
        /// Створена відкладена дія; інакше — відповідна доменна помилка,
        /// якщо запит не може бути прийнятий.
        ///
        /// (Created pending action; otherwise, the corresponding domain error
        /// when the request cannot be accepted.)
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
            {
                return Result<UserPendingAction>.Failure(resultAccess.Errors);
            }

            if (newLogin == Login)
            {
                return Result<UserPendingAction>.Failure(UserErrors.NoUpdateNeeded<User>());
            }

            var userAction = UserAction.From(newLogin);

            var requestResult = RequestPendingAction(
                userAction,
                now
            );

            if (requestResult.IsFailure)
            {
                return Result<UserPendingAction>.Failure(requestResult.Errors);
            }

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

            UpdatedAt = now;

            return Result<UserPendingAction>.Success(pendingAction);
        }
        #endregion

        #region RequestPasswordChange
        /// <summary>
        /// Створює запит на зміну пароля користувача.
        ///
        /// Перевіряє часову послідовність операції, доступ користувача,
        /// відповідність нового відкритого пароля переданому хешу
        /// та відмінність нового пароля від поточного.
        ///
        /// Невідповідність нового пароля його хешу вважається
        /// порушенням передумови операції. Збіг нового пароля
        /// з поточним повертається як доменна помилка.
        ///
        /// Успішний запит створює відкладену дію, яка потребує
        /// подальшого підтвердження, та записує доменну подію
        /// запиту зміни пароля.
        ///
        /// Поточний пароль користувача цим методом не змінюється.
        ///
        /// (Creates a request to change the user's password.
        ///
        /// Validates chronological consistency, user access,
        /// correspondence between the new plain password and the supplied hash,
        /// and whether the new password differs from the current password.
        ///
        /// A mismatch between the new password and its hash is treated
        /// as an operation precondition violation. A new password matching
        /// the current password is reported as a domain error.
        ///
        /// A successful request creates a pending action that requires
        /// subsequent confirmation and records a password-change
        /// request domain event.
        ///
        /// This method does not change the user's current password.)
        /// </summary>
        /// <param name="plainNewPassword">
        /// Новий пароль у відкритому вигляді, який використовується
        /// для перевірки переданого та поточного хешів.
        ///
        /// (New plain password used to check the supplied
        /// and current password hashes.)
        /// </param>
        /// <param name="newPassword">
        /// Новий пароль, представлений його хешем.
        ///
        /// (New password represented by its hash.)
        /// </param>
        /// <param name="verifier">
        /// Функція перевірки відповідності відкритого пароля заданому хешу.
        ///
        /// (Function that checks whether a plain password matches a given hash.)
        /// </param>
        /// <param name="now">
        /// Час перевірок і створення запиту.
        ///
        /// (Time used for validation and request creation.)
        /// </param>
        /// <returns>
        /// Створена відкладена дія; інакше — відповідна доменна помилка,
        /// якщо запит не може бути прийнятий.
        ///
        /// (Created pending action; otherwise, the corresponding domain error
        /// when the request cannot be accepted.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо переданий хеш нового пароля
        /// не відповідає переданому відкритому паролю.
        ///
        /// (Thrown when the supplied new password hash
        /// does not match the supplied plain password.)
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
            {
                return Result<UserPendingAction>.Failure(resultAccess.Errors);
            }

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
            {
                return Result<UserPendingAction>.Failure(UserErrors.NoUpdateNeeded<User>());
            }

            var userAction = UserAction.From(newPassword);

            var requestResult = RequestPendingAction(
                userAction,
                now
            );

            if (requestResult.IsFailure)
            {
                return Result<UserPendingAction>.Failure(requestResult.Errors);
            }

            var pendingAction = requestResult.Value;

            AddDomainEvent(new UserPasswordChangeRequestedEvent(
                Id,
                pendingAction.Id,
                Login,
                pendingAction.ConfirmationToken,
                pendingAction.ExpiresAt,
                now)
            );

            UpdatedAt = now;

            return Result<UserPendingAction>.Success(pendingAction);
        }
        #endregion

        #region RequestAccountDeletion
        /// <summary>
        /// Створює запит на видалення облікового запису користувача.
        ///
        /// Перевіряє часову послідовність операції, доступ користувача
        /// та допустимість переданої причини видалення.
        ///
        /// Після успішної перевірки створює відкладену дію,
        /// яка потребує подальшого підтвердження, та записує
        /// доменну подію запиту видалення облікового запису.
        ///
        /// Користувач цим методом не видаляється.
        ///
        /// (Creates a request to delete the user's account.
        ///
        /// Validates chronological consistency, user access,
        /// and the supplied deletion reason.
        ///
        /// After successful validation, creates a pending action
        /// that requires subsequent confirmation and records
        /// an account-deletion request domain event.
        ///
        /// This method does not delete the user.)
        /// </summary>
        /// <param name="reason">
        /// Необов’язкова причина видалення облікового запису.
        ///
        /// (Optional account-deletion reason.)
        /// </param>
        /// <param name="now">
        /// Час перевірок і створення запиту.
        ///
        /// (Time used for validation and request creation.)
        /// </param>
        /// <returns>
        /// Створена відкладена дія; інакше — відповідна доменна помилка
        /// перевірки доступу, причини видалення або створення запиту.
        ///
        /// (Created pending action; otherwise, the corresponding domain error
        /// from access validation, deletion-reason validation, or request creation.)
        /// </returns>
        public Result<UserPendingAction> RequestAccountDeletion(
            string? reason,
            DateTimeOffset now)
        {
            var resultAccess = ValidateAccessForPendingActionRequest(now);

            if (resultAccess.IsFailure)
            {
                return Result<UserPendingAction>.Failure(resultAccess.Errors);
            }

            var deletionReason = DeletionReason.Create(reason);

            if (deletionReason.IsFailure)
            {
                return Result<UserPendingAction>.Failure(deletionReason.Errors);
            }

            var userAction = UserAction.From(deletionReason.Value);

            var requestResult = RequestPendingAction(
                userAction,
                now
            );

            if (requestResult.IsFailure)
            {
                return Result<UserPendingAction>.Failure(requestResult.Errors);
            }

            var pendingAction = requestResult.Value;

            AddDomainEvent(new UserAccountDeletionRequestedEvent(
                Id,
                pendingAction.Id,
                Login,
                pendingAction.ConfirmationToken,
                pendingAction.ExpiresAt,
                now)
            );

            UpdatedAt = now;

            return Result<UserPendingAction>.Success(pendingAction);
        }
        #endregion

        #region Pending Action Request Processing
        /// <summary>
        /// Перевіряє часову послідовність запиту та доступ
        /// користувача для створення відкладеної дії.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Validates chronological consistency of the request
        /// and user access for creating a pending action.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="now">
        /// Час операції, відносно якого перевіряються
        /// часова послідовність і доступ користувача.
        ///
        /// (Operation time used to evaluate chronological consistency
        /// and user access.)
        /// </param>
        /// <returns>
        /// Успіх, якщо доступ дозволений; інакше — відповідна доменна помилка доступу.
        ///
        /// (Success if access is allowed; otherwise, the corresponding access domain error.)
        /// </returns>
        private Result ValidateAccessForPendingActionRequest(DateTimeOffset now)
        {
            EnsureValidOperationTimestamp(now);

            var access = ProvideAccess(now);

            if (access.IsFailure)
            {
                return Result.Failure(access.Errors);
            }

            return Result.Success();
        }

        /// <summary>
        /// Виконує спільний процес створення відкладеної дії.
        ///
        /// Готує план додавання нової дії або заміни попередньої
        /// дії із завершеним строком. Застосовує підготовлений план,
        /// оновлює час зміни користувача та записує зміни агрегату.
        ///
        /// Якщо підготовка плану повертає помилку,
        /// план не застосовується і стан агрегату не змінюється.
        ///
        /// Доменну подію відповідного сценарію записує
        /// публічний метод, який ініціював запит.
        ///
        /// (Executes the shared pending-action creation process.
        ///
        /// Prepares a plan to add a new action or replace a previous action
        /// whose lifetime has ended. Applies the prepared plan, updates
        /// the user's modification timestamp, and records aggregate changes.
        ///
        /// If plan preparation returns a failure,
        /// the plan is not applied and aggregate state remains unchanged.
        ///
        /// The scenario-specific public method that initiated the request
        /// records the domain event.)
        /// </summary>
        /// <param name="userAction">
        /// Дані дії, для якої створюється запит.
        ///
        /// (Action data for the request being created.)
        /// </param>
        /// <param name="now">
        /// Час створення нової дії, застосування плану
        /// та запису змін агрегату.
        ///
        /// (Time used for creating the new action, applying the plan,
        /// and recording aggregate changes.)
        /// </param>
        /// <returns>
        /// Нова відкладена дія після застосування плану;
        /// інакше — доменна помилка підготовки плану.
        ///
        /// (New pending action after applying the plan;
        /// otherwise, the domain error from plan preparation.)
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
        /// Якщо поточної дії немає, створює нову дію
        /// та план її додавання до агрегату.
        ///
        /// Якщо поточна дія активна у вказаний момент,
        /// повертає доменну помилку наявності активної дії.
        ///
        /// Інакше перевіряє можливість позначення попередньої дії
        /// простроченою, створює нову дію та готує план заміни.
        /// Позначення попередньої дії простроченою виконується
        /// під час подальшого застосування плану.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Determines and prepares a pending-action request plan.
        ///
        /// When no current action exists, creates a new action
        /// and a plan to add it to the aggregate.
        ///
        /// When the current action is active at the specified time,
        /// returns a domain error indicating that an active action already exists.
        ///
        /// Otherwise, validates that the previous action can be marked
        /// as expired, creates a new action, and prepares a replacement plan.
        /// The previous action is marked as expired
        /// when the plan is subsequently applied.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="userAction">
        /// Дані нової відкладеної дії.
        ///
        /// (Data for the new pending action.)
        /// </param>
        /// <param name="now">
        /// Час перевірки поточної дії та створення нової.
        ///
        /// (Time used to evaluate the current action and create the new one.)
        /// </param>
        /// <returns>
        /// Підготовлений план додавання або заміни дії;
        /// інакше — доменна помилка, якщо поточна дія ще активна.
        ///
        /// (Prepared action creation or replacement plan;
        /// otherwise, a domain error when the current action is still active.)
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
            {
                return Result<PendingActionRequestPlan>.Failure(UserErrors.ActionAlreadyExists<User>());
            }

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
        /// Stores the new action and defines separate stages
        /// for applying domain state and recording aggregate changes.)
        /// </summary>
        private abstract class PendingActionRequestPlan
        {
            /// <summary>
            /// Нова відкладена дія, яку план встановлює
            /// як поточну дію користувача.
            ///
            /// (New pending action that the plan sets
            /// as the user's current action.)
            /// </summary>
            public UserPendingAction Action { get; }

            protected PendingActionRequestPlan(UserPendingAction action)
            {
                Action = action;
            }

            /// <summary>
            /// Застосовує підготовлені планом зміни доменного стану.
            ///
            /// Метод не записує зміни агрегату або доменні події.
            /// Запис змін виконується окремим етапом після застосування плану.
            ///
            /// (Applies the domain-state changes prepared by the plan.
            ///
            /// The method does not record aggregate changes or domain events.
            /// Changes are recorded in a separate stage after applying the plan.)
            /// </summary>
            /// <param name="user">
            /// Користувач, до стану якого застосовується план.
            ///
            /// (User whose state the plan is applied to.)
            /// </param>
            /// <param name="now">
            /// Час операції, який використовується для часових переходів стану.
            ///
            /// (Operation time used for time-dependent state transitions.)
            /// </param>
            public abstract void Apply(
                User user,
                DateTimeOffset now);

            /// <summary>
            /// Записує зміни агрегату, що відповідають
            /// уже застосованим переходам доменного стану.
            ///
            /// Не виконує повторних переходів стану доменних сутностей
            /// і не записує доменні події.
            ///
            /// (Records aggregate changes corresponding
            /// to the already applied domain-state transitions.
            ///
            /// Does not repeat domain-entity state transitions
            /// or record domain events.)
            /// </summary>
            /// <param name="user">
            /// Користувач, до якого додаються записи змін агрегату.
            ///
            /// (User to which aggregate-change records are added.)
            /// </param>
            /// <param name="now">
            /// Час операції, що передається до записів змін.
            ///
            /// (Operation time passed to the change records.)
            /// </param>
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

            /// <summary>
            /// Встановлює нову відкладену дію як поточну дію користувача.
            ///
            /// Не записує зміни агрегату або доменні події.
            ///
            /// (Sets the new pending action as the user's current action.
            ///
            /// Does not record aggregate changes or domain events.)
            /// </summary>
            /// <param name="user">
            /// Користувач, якому встановлюється відкладена дія.
            ///
            /// (User whose pending action is set.)
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
                user.SetPendingAction(Action);
            }

            /// <summary>
            /// Записує зміну агрегату про створення нової відкладеної дії.
            ///
            /// Не змінює поточну дію користувача повторно
            /// і не записує доменні події.
            ///
            /// (Records an aggregate change for the newly created pending action.
            ///
            /// Does not set the user's current action again
            /// or record domain events.)
            /// </summary>
            /// <param name="user">
            /// Користувач, до якого додається запис про створення дії.
            ///
            /// (User to which the action-creation record is added.)
            /// </param>
            /// <param name="now">
            /// Час операції, що передається до запису зміни.
            ///
            /// (Operation time passed to the change record.)
            /// </param>
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
        /// Позначає попередню дію простроченою під час застосування.
        /// Окремо записує її оновлення та створення нової дії.
        ///
        /// (Plan for expiring the previous pending action
        /// and replacing it with a new action.
        ///
        /// Marks the previous action as expired when applied.
        /// Separately records its update and the creation of the new action.)
        /// </summary>
        private sealed class ReplaceExpiredPendingActionPlan : PendingActionRequestPlan
        {
            /// <summary>
            /// Попередня відкладена дія, строк якої завершився.
            ///
            /// План позначає її простроченою під час застосування,
            /// після чого записує відповідне оновлення.
            ///
            /// (Previous pending action whose lifetime has ended.
            ///
            /// The plan marks it as expired when applied
            /// and then records the corresponding update.)
            /// </summary>
            public UserPendingAction ExpiredAction { get; }

            public ReplaceExpiredPendingActionPlan(
                UserPendingAction newAction,
                UserPendingAction expiredAction)
                : base(newAction)
            {
                ExpiredAction = expiredAction;
            }

            /// <summary>
            /// Позначає попередню відкладену дію простроченою
            /// та встановлює нову дію як поточну.
            ///
            /// Не записує зміни агрегату або доменні події.
            ///
            /// (Marks the previous pending action as expired
            /// and sets the new action as the current one.
            ///
            /// Does not record aggregate changes or domain events.)
            /// </summary>
            /// <param name="user">
            /// Користувач, поточна відкладена дія якого замінюється.
            ///
            /// (User whose current pending action is replaced.)
            /// </param>
            /// <param name="now">
            /// Час, яким фіксується завершення попередньої дії.
            ///
            /// (Time recorded for the previous action's expiration.)
            /// </param>
            public override void Apply(
                User user,
                DateTimeOffset now)
            {
                ExpiredAction.MarkAsExpired(now);

                user.SetPendingAction(Action);
            }

            /// <summary>
            /// Записує зміни агрегату про оновлення попередньої
            /// відкладеної дії та створення нової.
            ///
            /// Спочатку записує оновлення простроченої дії,
            /// після чого — створення дії, що її замінила.
            ///
            /// Не виконує повторних переходів доменного стану
            /// і не записує доменні події.
            ///
            /// (Records aggregate changes for updating the previous
            /// pending action and creating the new one.
            ///
            /// Records the expired action's update first,
            /// followed by the creation of its replacement.
            ///
            /// Does not repeat domain-state transitions
            /// or record domain events.)
            /// </summary>
            /// <param name="user">
            /// Користувач, до якого додаються записи змін.
            ///
            /// (User to which the change records are added.)
            /// </param>
            /// <param name="now">
            /// Час операції, що передається до обох записів змін.
            ///
            /// (Operation time passed to both change records.)
            /// </param>
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
