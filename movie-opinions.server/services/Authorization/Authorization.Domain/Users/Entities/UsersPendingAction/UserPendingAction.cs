using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Contracts;
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
    public sealed class UserPendingAction : Entity<UserPendingActionId>, IUserOwned
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
        /// Ініціалізує нову відкладену дію з уже підготовленими
        /// ідентифікатором, токеном підтвердження та даними операції.
        ///
        /// Обчислює час завершення строку дії, встановлює початковий
        /// статус <see cref="ActionStatus.Active"/> та залишає часові
        /// позначки кінцевих переходів порожніми.
        ///
        /// Вхідні значення перевіряються фабричним методом
        /// <see cref="Create(UserId, UserAction, DateTimeOffset)"/>.
        ///
        /// (Initializes a new pending action using an already prepared
        /// identifier, confirmation token, and action data.
        ///
        /// Calculates the expiration time, assigns the initial
        /// <see cref="ActionStatus.Active"/> status, and leaves all terminal
        /// transition timestamps empty.
        ///
        /// Input values are validated by the
        /// <see cref="Create(UserId, UserAction, DateTimeOffset)"/> factory method.)
        /// </summary>
        /// <param name="userPendingActionId">Згенерований ідентифікатор відкладеної дії.</param>
        /// <param name="userId">Ідентифікатор користувача, якому належить дія.</param>
        /// <param name="confirmationFlowToken">Згенерований токен підтвердження.</param>
        /// <param name="userAction">Дані запланованої операції.</param>
        /// <param name="now">Час створення відкладеної дії.</param>
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

        /// <summary>
        /// Створює нову відкладену дію користувача.
        ///
        /// Перевіряє обов’язкові доменні об’єкти, генерує ідентифікатор
        /// сутності й токен підтвердження та створює дію у стані
        /// <see cref="ActionStatus.Active"/>.
        ///
        /// (Creates a new pending user action.
        ///
        /// Validates the required domain objects, generates the entity
        /// identifier and confirmation token, and creates the action
        /// in the <see cref="ActionStatus.Active"/> state.)
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача, якому належить дія.</param>
        /// <param name="userAction">Дані запланованої операції.</param>
        /// <param name="now">Час створення відкладеної дії.</param>
        /// <returns>Створена відкладена дія користувача.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язковий доменний об’єкт відсутній.
        /// </exception>
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
        /// Ініціалізує відкладену дію з уже перевіреного
        /// збереженого стану.
        ///
        /// Відтворює всі значення без генерації нових ідентифікаторів,
        /// токенів, часових позначок або доменних подій.
        ///
        /// Узгодженість переданого стану перевіряється фабричним методом
        /// <see cref="Restore(UserPendingActionId, UserId, ConfirmationFlowToken,
        /// UserAction, DateTimeOffset, DateTimeOffset?, DateTimeOffset?,
        /// DateTimeOffset?, ActionStatus, DateTimeOffset)"/>.
        ///
        /// (Initializes a pending action from an already validated
        /// persisted state.
        ///
        /// Restores all values without generating new identifiers,
        /// tokens, timestamps, or domain events.
        ///
        /// Consistency of the supplied state is validated by the
        /// Restore factory method.)
        /// </summary>
        /// <param name="userPendingActionId">Збережений ідентифікатор відкладеної дії.</param>
        /// <param name="userId">Ідентифікатор користувача, якому належить дія.</param>
        /// <param name="confirmationFlowToken">Збережений токен підтвердження.</param>
        /// <param name="userAction">Збережені дані запланованої операції.</param>
        /// <param name="expiresAt">Установлений час завершення строку дії.</param>
        /// <param name="confirmationTime">Час підтвердження або null.</param>
        /// <param name="expiredAt">Фактичний час переходу у стан Expired або null.</param>
        /// <param name="cancelledTime">Час скасування або null.</param>
        /// <param name="actionStatus">Збережений статус життєвого циклу дії.</param>
        /// <param name="createdAt">Час створення відкладеної дії.</param>
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

        /// <summary>
        /// Перевіряє та відновлює відкладену дію користувача
        /// зі збереженого стану.
        ///
        /// Перевіряє обов’язкові доменні об’єкти, допустимість статусу,
        /// часові межі та відповідність часових позначок поточному
        /// стану життєвого циклу.
        ///
        /// Метод не генерує нових ідентифікаторів або токенів
        /// і не створює доменних подій.
        ///
        /// (Validates and restores a pending user action
        /// from persisted state.
        ///
        /// Validates required domain objects, the status value,
        /// timestamp boundaries, and consistency between lifecycle
        /// timestamps and the current status.
        ///
        /// The method does not generate new identifiers or tokens
        /// and does not record domain events.)
        /// </summary>
        /// <param name="userPendingActionId">Збережений ідентифікатор відкладеної дії.</param>
        /// <param name="userId">Ідентифікатор користувача, якому належить дія.</param>
        /// <param name="confirmationToken">Збережений токен підтвердження.</param>
        /// <param name="userAction">Збережені дані запланованої операції.</param>
        /// <param name="expiresAt">Установлений час завершення строку дії.</param>
        /// <param name="confirmationTime">Час підтвердження або null.</param>
        /// <param name="expiredAt">Фактичний час переходу у стан Expired або null.</param>
        /// <param name="cancelledTime">Час скасування або null.</param>
        /// <param name="actionStatus">Збережений статус життєвого циклу дії.</param>
        /// <param name="createdAt">Час створення відкладеної дії.</param>
        /// <returns>Відновлена відкладена дія користувача.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язкові дані відсутні або статус не підтримується.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо часові значення або їх відповідність статусу
        /// утворюють неконсистентний стан.
        /// </exception>
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

        #region Queries
        /// <summary>
        /// Повертає час завершення періоду підтвердження дії.
        ///
        /// (Returns the expiration time of the action confirmation period.)
        /// </summary>
        /// <returns>Дата й час завершення строку дії.</returns>
        public DateTimeOffset GetExpirationDate()
        {
            return ExpiresAt;
        }

        /// <summary>
        /// Визначає, чи є дія активною у вказаний момент.
        ///
        /// Дія вважається активною, якщо вона перебуває у стані
        /// <see cref="ActionStatus.Active"/>, уже створена та її строк
        /// підтвердження ще не завершився.
        ///
        /// Метод не змінює стан сутності.
        ///
        /// (Determines whether the action is active at the specified time.
        ///
        /// An action is active when it has the <see cref="ActionStatus.Active"/>
        /// status, has already been created, and its confirmation period
        /// has not expired.
        ///
        /// The method does not modify entity state.)
        /// </summary>
        /// <param name="now">Час, відносно якого перевіряється активність дії.</param>
        /// <returns><see langword="true"/>, якщо дія активна; інакше — <see langword="false"/>.</returns>
        public bool IsActive(DateTimeOffset now)
        {
            return now >= CreatedAt
                && now < GetExpirationDate()
                && Status == ActionStatus.Active;
        }
        #endregion

        #region Behavior (Confirmation)
        /// <summary>
        /// Перевіряє, чи може активна відкладена дія бути підтверджена.
        ///
        /// Перевіряє поточний стан сутності, строк дії та відповідність
        /// токена підтвердження. Метод не змінює стан сутності.
        ///
        /// (Validates whether the active pending action can be confirmed.
        ///
        /// Validates the current entity state, expiration time, and confirmation
        /// token. The method does not modify entity state.)
        /// </summary>
        /// <param name="confirmationFlowToken">Токен, отриманий для підтвердження дії.</param>
        /// <param name="now">Час виконання перевірки.</param>
        /// <returns>
        /// Успіх, якщо дія може бути підтверджена; інакше — очікувана
        /// доменна помилка про завершення строку дії або неправильний токен.
        /// </returns>
        internal Result ValidateConfirmation(
            ConfirmationFlowToken confirmationFlowToken,
            DateTimeOffset now)
        {
            EnsureActiveState(now);

            if (now >= GetExpirationDate())
                return Result.Failure(PendingActionErrors.ExpiredAction<UserPendingAction>());

            return ValidateConfirmationToken(confirmationFlowToken);
        }

        /// <summary>
        /// Переводить активну відкладену дію у підтверджений стан.
        ///
        /// Перед мутацією повторно перевіряє всі передумови підтвердження.
        /// Після успішної перевірки встановлює статус
        /// <see cref="ActionStatus.Confirmed"/> і час підтвердження.
        ///
        /// (Transitions the active pending action to the confirmed state.
        ///
        /// Revalidates all confirmation preconditions before mutation.
        /// After successful validation, sets the
        /// <see cref="ActionStatus.Confirmed"/> status and confirmation time.)
        /// </summary>
        /// <param name="confirmationFlowToken">Токен підтвердження дії.</param>
        /// <param name="now">Час підтвердження дії.</param>
        internal void MarkAsConfirmed(
            ConfirmationFlowToken confirmationToken,
            DateTimeOffset now)
        {
            var validationResult = ValidateConfirmation(
                confirmationToken,
                now
            );

            if (validationResult.IsFailure)
            {
                throw DomainInvalidOperationException.PreconditionFailed<UserPendingAction>(
                    nameof(MarkAsConfirmed),
                    "A successfully validated pending-action confirmation.",
                    OperationType.Update,
                    context: new Dictionary<string, object>
                    {
                        ["PendingActionId"] = Id.Value,
                        ["UserId"] = UserId.Value,
                        ["ActionStatus"] = Status,
                        ["ExpiresAt"] = ExpiresAt,
                        ["CurrentTime"] = now,
                        ["ValidationErrorCodes"] = validationResult.Errors
                            .Select(x => x.Code)
                            .ToArray()
                    }
                );
            }

            Status = ActionStatus.Confirmed;
            ConfirmationTime = now;
        }
        #endregion

        #region Behavior (Cancellation)
        /// <summary>
        /// Перевіряє, чи може активна відкладена дія бути скасована.
        ///
        /// Перевіряє поточний стан сутності та відповідність токена.
        /// Завершення строку дії не перешкоджає скасуванню.
        /// Метод не змінює стан сутності.
        ///
        /// (Validates whether the active pending action can be cancelled.
        ///
        /// Validates the current entity state and confirmation token.
        /// Expiration of the confirmation period does not prevent cancellation.
        /// The method does not modify entity state.)
        /// </summary>
        /// <param name="confirmationFlowToken">Токен, що ідентифікує дію для скасування.</param>
        /// <param name="now">Час виконання перевірки.</param>
        /// <returns>
        /// Успіх, якщо дія може бути скасована; інакше — помилка
        /// про невідповідність токена.
        /// </returns>
        internal Result ValidateCancellation(
            ConfirmationFlowToken confirmationFlowToken,
            DateTimeOffset now)
        {
            EnsureActiveState(now);

            if (now >= GetExpirationDate())
                return Result.Failure(PendingActionErrors.ExpiredAction<UserPendingAction>());

            return ValidateConfirmationToken(confirmationFlowToken);
        }

        /// <summary>
        /// Переводить активну відкладену дію у скасований стан.
        ///
        /// Перед мутацією повторно перевіряє всі передумови скасування.
        /// Після успішної перевірки встановлює статус
        /// <see cref="ActionStatus.Cancelled"/> і час скасування.
        ///
        /// (Transitions the active pending action to the cancelled state.
        ///
        /// Revalidates all cancellation preconditions before mutation.
        /// After successful validation, sets the
        /// <see cref="ActionStatus.Cancelled"/> status and cancellation time.)
        /// </summary>
        /// <param name="confirmationFlowToken">Токен дії, що скасовується.</param>
        /// <param name="now">Час скасування дії.</param>
        internal void MarkAsCancelled(
            ConfirmationFlowToken confirmationToken,
            DateTimeOffset now)
        {
            var validationResult = ValidateCancellation(
                confirmationToken,
                now
            );

            if (validationResult.IsFailure)
            {
                throw DomainInvalidOperationException.PreconditionFailed<UserPendingAction>(
                    nameof(MarkAsCancelled),
                    "A successfully validated pending-action cancellation.",
                    OperationType.Update,
                    context: new Dictionary<string, object>
                    {
                        ["PendingActionId"] = Id.Value,
                        ["UserId"] = UserId.Value,
                        ["ActionStatus"] = Status,
                        ["ExpiresAt"] = ExpiresAt,
                        ["CurrentTime"] = now,
                        ["ValidationErrorCodes"] = validationResult.Errors
                            .Select(x => x.Code)
                            .ToArray()
                    }
                );
            }

            Status = ActionStatus.Cancelled;
            CancelledTime = now;
        }
        #endregion

        #region Behavior (Expire)
        /// <summary>
        /// Перевіряє, чи може активна відкладена дія бути позначена
        /// як прострочена.
        ///
        /// Перехід дозволений лише після досягнення встановленого часу
        /// завершення дії. Метод не змінює стан сутності.
        ///
        /// (Validates whether the active pending action can be marked
        /// as expired.
        ///
        /// The transition is allowed only after the configured expiration time
        /// has been reached. The method does not modify entity state.)
        /// </summary>
        /// <param name="now">Час, відносно якого перевіряється завершення строку дії.</param>
        internal void ValidateCanExpire(DateTimeOffset now)
        {
            EnsureActiveState(now);

            if (now < GetExpirationDate())
            {
                throw DomainInvalidOperationException.PreconditionFailed<UserPendingAction>(
                    nameof(ValidateCanExpire),
                    "The pending action expiration time must be reached.",
                    OperationType.Update,
                    context: new Dictionary<string, object>
                    {
                        ["PendingActionId"] = Id.Value,
                        ["ExpiresAt"] = ExpiresAt,
                        ["CurrentTime"] = now
                    }
                );
            }
        }

        /// <summary>
        /// Переводить активну відкладену дію у прострочений стан.
        ///
        /// Перед мутацією повторно перевіряє, що строк дії завершився.
        /// Після успішної перевірки встановлює статус
        /// <see cref="ActionStatus.Expired"/>.
        ///
        /// (Transitions the active pending action to the expired state.
        ///
        /// Revalidates that the action has expired before mutation.
        /// After successful validation, sets the
        /// <see cref="ActionStatus.Expired"/> status.)
        /// </summary>
        /// <param name="now">Час завершення строку дії.</param>
        internal void MarkAsExpired(DateTimeOffset now)
        {
            ValidateCanExpire(now);

            Status = ActionStatus.Expired;
            ExpiredAt = now;
        }
        #endregion

        #region Behavior
        /// <summary>
        /// Перевіряє спільні передумови переходу активної відкладеної дії.
        ///
        /// Перевіряє часову послідовність, допустимість значення статусу
        /// та те, що дія перебуває у стані <see cref="ActionStatus.Active"/>.
        ///
        /// Метод не змінює стан сутності.
        ///
        /// (Validates the common preconditions for transitioning an active
        /// pending action.
        ///
        /// Validates chronological consistency, the status value, and verifies
        /// that the action has the <see cref="ActionStatus.Active"/> status.
        ///
        /// The method does not modify entity state.)
        /// </summary>
        /// <param name="now">Час виконання операції.</param>
        private void EnsureActiveState(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<UserPendingAction>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            DomainGuard.AgainstUndefinedEnum<UserPendingAction>(
                OperationType.Update,
                (Status, nameof(Status))
            );

            if (Status != ActionStatus.Active)
            {
                throw DomainInvariantViolationException.BrokenState<UserPendingAction>(
                    "Only an active pending action can transition to another state.",
                    new Dictionary<string, object?>
                    {
                        ["PendingActionId"] = Id.Value,
                        ["UserId"] = UserId.Value,
                        ["ActionStatus"] = Status,
                        ["ExpiresAt"] = ExpiresAt,
                        ["CurrentTime"] = now
                    },
                    OperationType.Update
                );
            }
        }

        /// <summary>
        /// Перевіряє відповідність переданого токена токену поточної дії.
        ///
        /// Метод не змінює стан сутності.
        ///
        /// (Validates that the supplied token matches the token
        /// of the current action.
        ///
        /// The method does not modify entity state.)
        /// </summary>
        /// <param name="confirmationFlowToken">Токен, який необхідно перевірити.</param>
        /// <returns>
        /// Успіх, якщо токени збігаються; інакше — доменна помилка
        /// про неправильний токен.
        /// </returns>
        private Result ValidateConfirmationToken(ConfirmationFlowToken confirmationFlowToken)
        {
            DomainGuard.AgainstNull<UserPendingAction>(
                OperationType.Update,
                (confirmationFlowToken, nameof(confirmationFlowToken))
            );

            if (confirmationFlowToken != ConfirmationToken)
                return Result.Failure(PendingActionErrors.InvalidConfirmationToken<UserPendingAction>());

            return Result.Success();
        }
        #endregion

        #region Validation
        /// <summary>
        /// Перевіряє цілісність відновленого стану відкладеної дії.
        ///
        /// Перевіряє допустимість статусу, часові межі та узгодженість
        /// статусу з часовими позначками підтвердження, скасування
        /// і завершення строку дії.
        ///
        /// Для активної дії всі часові позначки переходів мають бути відсутні.
        /// Для кожного кінцевого статусу має бути встановлена лише відповідна
        /// часова позначка.
        ///
        /// (Validates the integrity of a restored pending-action state.
        ///
        /// Validates the status value, timestamp boundaries, and consistency
        /// between the status and the confirmation, cancellation, and expiration
        /// timestamps.
        ///
        /// An active action must not contain transition timestamps.
        /// Each terminal status must contain only its corresponding timestamp.)
        /// </summary>
        /// <param name="status">Відновлений статус дії.</param>
        /// <param name="createdAt">Час створення дії.</param>
        /// <param name="expiresAt">Запланований час завершення строку дії.</param>
        /// <param name="confirmationTime">Час підтвердження, якщо дія підтверджена.</param>
        /// <param name="cancelledTime">Час скасування, якщо дію скасовано.</param>
        /// <param name="expiredAt">Фактичний час переведення у прострочений стан.</param>
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

                _ => false
            };

            if (!isValidState)
            {
                throw DomainInvariantViolationException.BrokenState<UserPendingAction>(
                    $"Dates conflict. ConfirmationTime: {confirmationTime?.ToString() ?? "null"}, " +
                    $"ExpiredAt: {expiredAt?.ToString() ?? "null"}",
                    new Dictionary<string, object?>
                    {
                        ["PendingActionId"] = userPendingActionId,
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
