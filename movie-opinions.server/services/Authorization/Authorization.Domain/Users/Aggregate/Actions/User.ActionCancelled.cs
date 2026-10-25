using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Action;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.Errors;

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        /// <summary>
        /// Скасовує поточну відкладену дію користувача
        /// за її ідентифікатором і токеном підтвердження.
        ///
        /// Перевіряє доступ користувача, відповідність ідентифікатора
        /// поточній дії та виконання передумов скасування.
        /// Після успішної перевірки переводить дію у стан Cancelled,
        /// оновлює UpdatedAt, прибирає дію з поля поточної дії
        /// та записує її оновлення до змін агрегату.
        ///
        /// (Cancels the user's current pending action
        /// using its identifier and confirmation token.
        ///
        /// Checks user access, verifies that the identifier matches
        /// the current action, and validates cancellation preconditions.
        /// After successful validation, transitions the action to Cancelled,
        /// updates UpdatedAt, clears the current-action field,
        /// and records the action update among the aggregate changes.)
        /// </summary>
        /// <remarks>
        /// Скасування застосовується незалежно від типу відкладеної дії.
        /// Дія повинна мати статус Active, її строк дії не повинен
        /// завершитися, а переданий токен повинен відповідати токену дії.
        ///
        /// Скасована сутність передається у UserPendingActionUpdated
        /// для подальшого збереження її кінцевого стану.
        /// Очікувана відмова не спричиняє скасування дії.
        ///
        /// (Cancellation applies regardless of the pending action type.
        /// The action must have Active status, its lifetime must not
        /// have elapsed, and the supplied token must match the action token.
        ///
        /// The cancelled entity is included in UserPendingActionUpdated
        /// for subsequent persistence of its final state.
        /// An expected failure does not cancel the action.)
        /// </remarks>
        /// <param name="confirmationToken">
        /// Токен поточної відкладеної дії, необхідний для її скасування.
        ///
        /// (The current pending action token required for cancellation.)
        /// </param>
        /// <param name="actionId">
        /// Ідентифікатор відкладеної дії, яку потрібно скасувати.
        ///
        /// (The identifier of the pending action to cancel.)
        /// </param>
        /// <param name="now">
        /// Час перевірки доступу, скасування дії
        /// та оновлення агрегату.
        ///
        /// (The timestamp used for access validation,
        /// action cancellation, and the aggregate update.)
        /// </param>
        /// <returns>
        /// Успіх після скасування дії та реєстрації зміни агрегату;
        /// інакше — помилка доступу, відсутності поточної дії,
        /// невідповідності її ідентифікатора, завершення строку дії
        /// або невідповідності токена.
        ///
        /// (Success after cancelling the action and recording
        /// the aggregate change; otherwise, an access-denied,
        /// missing-action, action-identifier-mismatch, expiration,
        /// or token-mismatch error.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов'язковий аргумент дорівнює null,
        /// час операції порушує часові вимоги агрегату чи дії
        /// або поточний статус дії не підтримується.
        ///
        /// (Thrown when a required argument is null,
        /// the operation time violates the aggregate or action
        /// timestamp requirements, or the action status is unsupported.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо поточна відкладена дія
        /// не перебуває у стані Active.
        ///
        /// (Thrown when the current pending action
        /// is not in the Active state.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо повторна перевірка передумов
        /// у MarkAsCancelled повернула доменну помилку
        /// після успішної попередньої перевірки.
        ///
        /// (Thrown when precondition revalidation in MarkAsCancelled
        /// returns a domain error after successful initial validation.)
        /// </exception>
        public Result CancelledAction(
            ConfirmationFlowToken confirmationToken,
            UserPendingActionId actionId,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (confirmationToken, nameof(confirmationToken)),
                (actionId, nameof(actionId))
            );

            EnsureValidOperationTimestamp(now);

            var access = ProvideAccess(now);

            if (access.IsFailure)
                return access;

            var currentAction = _action;

            if (currentAction is null)
                return Result.Failure(UserErrors.EmptyAction<User>());

            if (currentAction.Id != actionId)
                return Result.Failure(UserErrors.NotFoundAction<User>());

            var cancellationValidation = currentAction.ValidateCancellation(
                confirmationToken,
                now
            );

            if (cancellationValidation.IsFailure)
                return cancellationValidation;

            currentAction.MarkAsCancelled(
                confirmationToken,
                now
            );

            UpdatedAt = now;

            _action = null;

            AddAggregateChange(new UserPendingActionUpdated(
                currentAction,
                now)
            );

            return Result.Success();
        } 
    }
}
