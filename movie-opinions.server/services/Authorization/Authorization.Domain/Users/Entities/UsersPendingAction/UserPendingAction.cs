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
        /// <summary>
        /// Тривалість життя активної дії в хвилинах.
        ///
        /// (Duration of the active action lifetime in minutes.)
        /// </summary>
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
        /// Ініціалізує нову відкладену дію з підготовлених доменних значень,
        /// установлює строк дії та початковий статус Active.
        /// Часові позначки кінцевих переходів залишаються порожніми.
        ///
        /// (Initializes a new pending action from prepared domain values,
        /// sets its lifetime and initial Active status,
        /// and leaves terminal transition timestamps unset.)
        /// </summary>
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
        /// і токен потоку підтвердження та встановлює початковий статус Active.
        ///
        /// (Creates a new pending user action.
        ///
        /// Validates required domain objects, generates the identifier
        /// and confirmation-flow token, and sets the initial Active status.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача, якому належить дія.
        ///
        /// (Identifier of the user who owns the action.)
        /// </param>
        /// <param name="userAction">
        /// Дані запланованої операції.
        ///
        /// (The planned operation data.)
        /// </param>
        /// <param name="now">
        /// Час створення відкладеної дії.
        ///
        /// (The pending action creation time.)
        /// </param>
        /// <returns>
        /// Створена активна відкладена дія користувача.
        ///
        /// (The newly created active pending user action.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язковий доменний об’єкт відсутній.
        ///
        /// (Thrown when a required domain object is missing.)
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
        /// Ініціалізує відкладену дію зі збережених значень,
        /// попередньо перевірених фабрикою Restore.
        ///
        /// (Initializes a pending action from persisted values
        /// previously validated by the Restore factory.)
        /// </summary>
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
        /// часові межі та узгодженість статусу з часовими позначками.
        ///
        /// (Validates and restores a pending user action
        /// from its persisted state.
        ///
        /// Validates required domain objects, the status value,
        /// timestamp boundaries, and consistency between the status and timestamps.)
        /// </summary>
        /// <param name="userPendingActionId">
        /// Збережений ідентифікатор відкладеної дії.
        ///
        /// (The persisted pending action identifier.)
        /// </param>
        /// <param name="userId">
        /// Ідентифікатор користувача, якому належить дія.
        ///
        /// (Identifier of the user who owns the action.)
        /// </param>
        /// <param name="confirmationToken">
        /// Збережений токен потоку підтвердження.
        ///
        /// (The persisted confirmation-flow token.)
        /// </param>
        /// <param name="userAction">
        /// Збережені дані запланованої операції.
        ///
        /// (The persisted planned operation data.)
        /// </param>
        /// <param name="expiresAt">
        /// Установлений час завершення строку дії.
        ///
        /// (The configured action expiration time.)
        /// </param>
        /// <param name="confirmationTime">
        /// Час підтвердження або null.
        ///
        /// (The confirmation time, or null.)
        /// </param>
        /// <param name="expiredAt">
        /// Фактичний час переходу у стан Expired або null.
        ///
        /// (The actual time of transition to Expired, or null.)
        /// </param>
        /// <param name="cancelledTime">
        /// Час скасування або null.
        ///
        /// (The cancellation time, or null.)
        /// </param>
        /// <param name="actionStatus">
        /// Збережений статус життєвого циклу дії.
        ///
        /// (The persisted action lifecycle status.)
        /// </param>
        /// <param name="createdAt">
        /// Час створення відкладеної дії.
        ///
        /// (The pending action creation time.)
        /// </param>
        /// <returns>
        /// Відновлена відкладена дія користувача.
        ///
        /// (The restored pending user action.)
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
        /// <returns>
        /// Дата й час завершення строку дії.
        ///
        /// (The action expiration date and time.)
        /// </returns>
        public DateTimeOffset GetExpirationDate()
        {
            return ExpiresAt;
        }

        /// <summary>
        /// Визначає, чи є дія активною у вказаний момент.
        ///
        /// Дія вважається активною, якщо вона має статус Active,
        /// уже створена та її період підтвердження ще не завершився.
        ///
        /// Метод не змінює стан сутності.
        ///
        /// (Determines whether the action is active at the specified time.
        ///
        /// An action is active when its status is Active, it has already
        /// been created, and its confirmation period has not expired.
        ///
        /// The method does not modify entity state.)
        /// </summary>
        /// <param name="now">
        /// Час, відносно якого перевіряється активність дії.
        ///
        /// (The point in time used to check whether the action is active.)
        /// </param>
        /// <returns>
        /// true, якщо дія активна у вказаний момент; інакше false.
        ///
        /// (true when the action is active at the specified time; otherwise, false.)
        /// </returns>
        public bool IsActive(DateTimeOffset now)
        {
            return now >= CreatedAt
                && now < GetExpirationDate()
                && Status == ActionStatus.Active;
        }
        #endregion

        #region Behavior (Confirmation)
        /// <summary>
        /// Перевіряє можливість підтвердження відкладеної дії
        /// зі статусом Active.
        ///
        /// Перевіряє строк дії та відповідність токена підтвердження.
        /// Метод не змінює стан сутності.
        ///
        /// (Validates whether a pending action with Active status can be confirmed.
        ///
        /// Checks the expiration time and confirmation token.
        /// The method does not modify entity state.)
        /// </summary>
        /// <param name="confirmationFlowToken">
        /// Токен, отриманий для підтвердження дії.
        ///
        /// (The token supplied to confirm the action.)
        /// </param>
        /// <param name="now">
        /// Час виконання перевірки.
        ///
        /// (The time at which validation is performed.)
        /// </param>
        /// <returns>
        /// Успішний результат, якщо дію можна підтвердити;
        /// інакше помилка завершення строку дії або невідповідності токена.
        ///
        /// (A successful result when the action can be confirmed;
        /// otherwise, an expiration or token-mismatch error.)
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо дія не перебуває у стані Active.
        ///
        /// (Thrown when the action is not in the Active state.)
        /// </exception>
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
        /// Повторно перевіряє передумови підтвердження, після чого
        /// встановлює статус Confirmed і час підтвердження.
        ///
        /// (Transitions an active pending action to the confirmed state.
        ///
        /// Revalidates confirmation preconditions, then sets
        /// the Confirmed status and confirmation time.)
        /// </summary>
        /// <param name="confirmationToken">
        /// Токен підтвердження дії.
        ///
        /// (The action confirmation token.)
        /// </param>
        /// <param name="now">
        /// Час підтвердження дії.
        ///
        /// (The action confirmation time.)
        /// </param>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо повторна перевірка підтвердження
        /// повернула очікувану доменну помилку.
        ///
        /// (Thrown when confirmation revalidation
        /// returns an expected domain error.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо дія не перебуває у стані Active.
        ///
        /// (Thrown when the action is not in the Active state.)
        /// </exception>
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
        /// Перевіряє можливість скасування відкладеної дії
        /// зі статусом Active.
        ///
        /// Перевіряє строк дії та відповідність токена.
        /// Скасування дозволене лише до завершення періоду підтвердження.
        /// Метод не змінює стан сутності.
        ///
        /// (Validates whether a pending action with Active status can be cancelled.
        ///
        /// Checks the expiration time and confirmation token.
        /// Cancellation is allowed only before the confirmation period expires.
        /// The method does not modify entity state.)
        /// </summary>
        /// <param name="confirmationFlowToken">
        /// Токен, що ідентифікує дію для скасування.
        ///
        /// (The token identifying the action to cancel.)
        /// </param>
        /// <param name="now">
        /// Час виконання перевірки.
        ///
        /// (The time at which validation is performed.)
        /// </param>
        /// <returns>
        /// Успішний результат, якщо дію можна скасувати;
        /// інакше помилка завершення строку дії або невідповідності токена.
        ///
        /// (A successful result when the action can be cancelled;
        /// otherwise, an expiration or token-mismatch error.)
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо дія не перебуває у стані Active.
        ///
        /// (Thrown when the action is not in the Active state.)
        /// </exception>
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
        /// Повторно перевіряє передумови скасування, після чого
        /// встановлює статус Cancelled і час скасування.
        ///
        /// (Transitions an active pending action to the cancelled state.
        ///
        /// Revalidates cancellation preconditions, then sets
        /// the Cancelled status and cancellation time.)
        /// </summary>
        /// <param name="confirmationToken">
        /// Токен дії, що скасовується.
        ///
        /// (The token of the action being cancelled.)
        /// </param>
        /// <param name="now">
        /// Час скасування дії.
        ///
        /// (The action cancellation time.)
        /// </param>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо повторна перевірка скасування
        /// повернула очікувану доменну помилку.
        ///
        /// (Thrown when cancellation revalidation
        /// returns an expected domain error.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо дія не перебуває у стані Active.
        ///
        /// (Thrown when the action is not in the Active state.)
        /// </exception>
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
        /// Перевіряє можливість переведення активної відкладеної дії
        /// у стан Expired.
        ///
        /// Перехід дозволений, коли час завершення строку дії вже настав.
        /// Метод не змінює стан сутності.
        ///
        /// (Validates whether an active pending action can transition to Expired.
        ///
        /// The transition is allowed when the expiration time has been reached.
        /// The method does not modify entity state.)
        /// </summary>
        /// <param name="now">
        /// Час, відносно якого перевіряється завершення строку дії.
        ///
        /// (The point in time used to check expiration.)
        /// </param>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо час завершення строку дії ще не настав.
        ///
        /// (Thrown when the expiration time has not yet been reached.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо дія не перебуває у стані Active.
        ///
        /// (Thrown when the action is not in the Active state.)
        /// </exception>
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
        /// Повторно перевіряє передумови переходу, встановлює статус Expired
        /// та записує фактичний час переходу в ExpiredAt.
        ///
        /// (Transitions an active pending action to the expired state.
        ///
        /// Revalidates transition preconditions, sets the Expired status,
        /// and records the actual transition time in ExpiredAt.)
        /// </summary>
        /// <param name="now">
        /// Фактичний час переведення дії у прострочений стан.
        ///
        /// (The actual time at which the action is marked as expired.)
        /// </param>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо час завершення строку дії ще не настав.
        ///
        /// (Thrown when the expiration time has not yet been reached.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо дія не перебуває у стані Active.
        ///
        /// (Thrown when the action is not in the Active state.)
        /// </exception>
        internal void MarkAsExpired(DateTimeOffset now)
        {
            ValidateCanExpire(now);

            Status = ActionStatus.Expired;
            ExpiredAt = now;
        }
        #endregion

        #region Behavior
        /// <summary>
        /// Перевіряє часову послідовність операції, допустимість статусу
        /// та перебування дії у стані Active.
        ///
        /// Строк дії перевіряється окремо в методах конкретних переходів.
        /// Метод не змінює стан сутності.
        ///
        /// (Validates the operation chronology, the status value,
        /// and that the action is in the Active state.
        ///
        /// Expiration is validated separately by the individual transition methods.
        /// The method does not modify entity state.)
        /// </summary>
        /// <param name="now">
        /// Час виконання операції.
        ///
        /// (The operation time.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує часу створення дії
        /// або поточний статус не підтримується.
        ///
        /// (Thrown when the operation time precedes the action creation time
        /// or the current status is unsupported.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо дія не перебуває у стані Active.
        ///
        /// (Thrown when the action is not in the Active state.)
        /// </exception>
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
        /// (Validates that the supplied token matches the current action token.
        ///
        /// The method does not modify entity state.)
        /// </summary>
        /// <param name="confirmationFlowToken">
        /// Токен, який необхідно перевірити.
        ///
        /// (The token to validate.)
        /// </param>
        /// <returns>
        /// Успішний результат, якщо токени збігаються;
        /// інакше помилка невідповідності токена.
        ///
        /// (A successful result when the tokens match;
        /// otherwise, a token-mismatch error.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо переданий токен відсутній.
        ///
        /// (Thrown when the supplied token is missing.)
        /// </exception>
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
        /// Перевіряє цілісність збереженого стану відкладеної дії.
        ///
        /// Перевіряє допустимість статусу, часові межі та узгодженість
        /// статусу з позначками підтвердження, скасування і прострочення.
        ///
        /// Для Active всі позначки переходів мають бути відсутні.
        /// Для кожного кінцевого статусу має бути встановлена лише
        /// відповідна часова позначка в допустимих межах.
        ///
        /// (Validates the integrity of the persisted pending-action state.
        ///
        /// Validates the status value, timestamp boundaries, and consistency
        /// between the status and confirmation, cancellation, and expiration timestamps.
        ///
        /// Active actions must not contain transition timestamps.
        /// Each terminal status must contain only its corresponding timestamp
        /// within the permitted boundaries.)
        /// </summary>
        /// <param name="userPendingActionId">
        /// Ідентифікатор дії для діагностичного контексту.
        ///
        /// (The action identifier used in diagnostic context.)
        /// </param>
        /// <param name="actionStatus">
        /// Збережений статус дії.
        ///
        /// (The persisted action status.)
        /// </param>
        /// <param name="expiresAt">
        /// Запланований час завершення строку дії.
        ///
        /// (The configured action expiration time.)
        /// </param>
        /// <param name="confirmationTime">
        /// Час підтвердження або null.
        ///
        /// (The confirmation time, or null.)
        /// </param>
        /// <param name="expiredAt">
        /// Фактичний час переходу у стан Expired або null.
        ///
        /// (The actual time of transition to Expired, or null.)
        /// </param>
        /// <param name="createdAt">
        /// Час створення дії.
        ///
        /// (The action creation time.)
        /// </param>
        /// <param name="cancelledTime">
        /// Час скасування або null.
        ///
        /// (The cancellation time, or null.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережений статус не підтримується.
        ///
        /// (Thrown when the persisted status is unsupported.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо часові межі або узгодженість статусу
        /// з часовими позначками порушені.
        ///
        /// (Thrown when timestamp boundaries or consistency
        /// between the status and timestamps are violated.)
        /// </exception>
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
