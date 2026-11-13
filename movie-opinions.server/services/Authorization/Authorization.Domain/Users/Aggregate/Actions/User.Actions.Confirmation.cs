using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Action;
using Authorization.Domain.Users.DomainEvents.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.Errors;

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        /// <summary>
        /// Підтверджує відкладену дію зміни логіна.
        ///
        /// Перевіряє часову послідовність операції, доступ користувача,
        /// наявність дії, відповідність її ідентифікатора та типу,
        /// строк дії і токен підтвердження.
        ///
        /// Після успішної перевірки змінює логін, позначає дію
        /// підтвердженою, прибирає її з активного стану агрегату,
        /// оновлює час зміни користувача та записує зміну дочірньої сутності.
        /// Додатково записує доменну подію з попереднім та новим логіном.
        ///
        /// Якщо перевірка підтвердження повертає помилку,
        /// логін не змінюється і подія зміни логіна не записується.
        ///
        /// (Confirms a pending login-change action.
        ///
        /// Validates chronological consistency, user access, action existence,
        /// identifier and type correspondence, expiration, and confirmation token.
        ///
        /// After successful validation, changes the login, marks the action
        /// as confirmed, removes it from the aggregate's active state,
        /// updates the user's modification timestamp, and records
        /// the child-entity change. Also records a domain event containing
        /// the previous and new login.
        ///
        /// If confirmation validation returns a failure,
        /// the login is not changed and no login-change event is recorded.)
        /// </summary>
        /// <param name="confirmationToken">
        /// Токен підтвердження відкладеної дії.
        ///
        /// (Pending-action confirmation token.)
        /// </param>
        /// <param name="actionId">
        /// Ідентифікатор дії, яку необхідно підтвердити.
        ///
        /// (Identifier of the action to confirm.)
        /// </param>
        /// <param name="now">
        /// Час перевірок і підтвердження зміни логіна.
        ///
        /// (Time used for validation and login-change confirmation.)
        /// </param>
        /// <returns>
        /// Успіх після зміни логіна та підтвердження дії;
        /// інакше — відповідна доменна помилка.
        ///
        /// (Success after changing the login and confirming the action;
        /// otherwise, the corresponding domain error.)
        /// </returns>
        public Result ConfirmLoginChange(
            ConfirmationFlowToken confirmationToken,
            UserPendingActionId actionId,
            DateTimeOffset now)
        {
            var previousLogin = Login;

            var result = ConfirmPendingAction<ChangeLoginAction>(
                confirmationToken,
                actionId,
                now,
                change =>
                {
                    Login = change.NewLogin;

                    return Result.Success();
                }
            );

            if (result.IsFailure)
            {
                return result;
            }

            AddDomainEvent(new UserLoginChangeConfirmedEvent(
                Id,
                actionId,
                previousLogin,
                Login,
                now)
            );

            UpdatedAt = now;

            return Result.Success();
        }

        /// <summary>
        /// Підтверджує відкладену дію зміни пароля.
        ///
        /// Перевіряє часову послідовність операції, доступ користувача,
        /// наявність дії, відповідність її ідентифікатора та типу,
        /// строк дії і токен підтвердження.
        ///
        /// Після успішної перевірки змінює пароль, позначає дію
        /// підтвердженою, прибирає її з активного стану агрегату,
        /// оновлює час зміни користувача та записує зміну дочірньої сутності.
        /// Додатково записує доменну подію підтвердженої зміни пароля.
        /// Подія не містить пароля або його хешу.
        ///
        /// Якщо перевірка підтвердження повертає помилку,
        /// пароль не змінюється і подія зміни пароля не записується.
        ///
        /// (Confirms a pending password-change action.
        ///
        /// Validates chronological consistency, user access, action existence,
        /// identifier and type correspondence, expiration, and confirmation token.
        ///
        /// After successful validation, changes the password, marks the action
        /// as confirmed, removes it from the aggregate's active state,
        /// updates the user's modification timestamp, and records
        /// the child-entity change. Also records a password-change confirmation
        /// domain event. The event contains neither the password nor its hash.
        ///
        /// If confirmation validation returns a failure,
        /// the password is not changed and no password-change event is recorded.)
        /// </summary>
        /// <param name="confirmationToken">
        /// Токен підтвердження відкладеної дії.
        ///
        /// (Pending-action confirmation token.)
        /// </param>
        /// <param name="actionId">
        /// Ідентифікатор дії, яку необхідно підтвердити.
        ///
        /// (Identifier of the action to confirm.)
        /// </param>
        /// <param name="now">
        /// Час перевірок і підтвердження зміни пароля.
        ///
        /// (Time used for validation and password-change confirmation.)
        /// </param>
        /// <returns>
        /// Успіх після зміни пароля та підтвердження дії;
        /// інакше — відповідна доменна помилка.
        ///
        /// (Success after changing the password and confirming the action;
        /// otherwise, the corresponding domain error.)
        /// </returns>
        public Result ConfirmPasswordChange(
            ConfirmationFlowToken confirmationToken,
            UserPendingActionId actionId,
            DateTimeOffset now)
        {
            var result = ConfirmPendingAction<ChangePasswordAction>(
                confirmationToken,
                actionId,
                now,
                change =>
                {
                    Password = change.NewPassword;

                    return Result.Success();
                }
            );

            if (result.IsFailure)
            {
                return result;
            }

            AddDomainEvent(new UserPasswordChangeConfirmedEvent(
                Id,
                actionId,
                Login,
                now)
            );

            UpdatedAt = now;

            return Result.Success();
        }

        /// <summary>
        /// Підтверджує відкладену дію видалення облікового запису.
        ///
        /// Перевіряє часову послідовність операції, доступ користувача,
        /// наявність дії, відповідність її ідентифікатора та типу,
        /// строк дії і токен підтвердження.
        ///
        /// Після успішної перевірки застосовує видалення користувача,
        /// позначає дію підтвердженою, прибирає її з активного стану
        /// агрегату, оновлює час зміни користувача та записує
        /// зміну дочірньої сутності.
        ///
        /// Записує доменну подію підтвердженого видалення,
        /// що містить логін користувача та кінцевий строк відновлення.
        ///
        /// Успішне застосування видалення повинно створити
        /// стан <c>UserDeletion</c>. Його відсутність після успішного
        /// підтвердження вважається порушенням інваріанта.
        ///
        /// Якщо перевірка або застосування видалення повертає помилку,
        /// дія не позначається підтвердженою і подія видалення не записується.
        ///
        /// (Confirms a pending account-deletion action.
        ///
        /// Validates chronological consistency, user access, action existence,
        /// identifier and type correspondence, expiration, and confirmation token.
        ///
        /// After successful validation, applies the user deletion,
        /// marks the action as confirmed, removes it from the aggregate's
        /// active state, updates the user's modification timestamp,
        /// and records the child-entity change.
        ///
        /// Records an account-deletion confirmation domain event
        /// containing the user's login and restoration deadline.
        ///
        /// Successful deletion must create the <c>UserDeletion</c> state.
        /// Its absence after successful confirmation is treated
        /// as an invariant violation.
        ///
        /// If validation or deletion returns a failure, the action
        /// is not marked as confirmed and no deletion event is recorded.)
        /// </summary>
        /// <param name="confirmationToken">
        /// Токен підтвердження відкладеної дії.
        ///
        /// (Pending-action confirmation token.)
        /// </param>
        /// <param name="actionId">
        /// Ідентифікатор дії, яку необхідно підтвердити.
        ///
        /// (Identifier of the action to confirm.)
        /// </param>
        /// <param name="now">
        /// Час перевірок, видалення користувача та підтвердження дії.
        ///
        /// (Time used for validation, user deletion, and action confirmation.)
        /// </param>
        /// <returns>
        /// Успіх після видалення користувача та підтвердження дії;
        /// інакше — відповідна доменна помилка перевірки або видалення.
        ///
        /// (Success after deleting the user and confirming the action;
        /// otherwise, the corresponding validation or deletion domain error.)
        /// </returns>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо підтвердження видалення завершилося успішно,
        /// але стан видалення користувача не був створений.
        ///
        /// (Thrown when deletion confirmation succeeds,
        /// but the user-deletion state has not been created.)
        /// </exception>
        public Result ConfirmAccountDeletion(
            ConfirmationFlowToken confirmationToken,
            UserPendingActionId actionId,
            DateTimeOffset now)
        {
            var login = Login;

            var result = ConfirmPendingAction<DeleteUserAction>(
                confirmationToken,
                actionId,
                now,
                change => Delete(
                    now,
                    change.Reason
                )
            );

            if (result.IsFailure)
            {
                return result;
            }

            var deletion = _deletion;

            if (deletion is null)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "The account deletion was successfully confirmed, " +
                    "but the deletion state was not created.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = Id.Value,
                        ["PendingActionId"] = actionId.Value,
                        ["ConfirmedAt"] = now
                    },
                    OperationType.Update
                );
            }

            AddDomainEvent(new UserAccountDeletionConfirmedEvent(
                Id,
                actionId,
                login,
                deletion.RestoreUntil,
                now)
            );

            UpdatedAt = now;

            return Result.Success();
        }

        /// <summary>
        /// Виконує спільний процес підтвердження відкладеної дії.
        ///
        /// Перевіряє обов’язкові аргументи, часову послідовність операції,
        /// доступ користувача, наявність дії, відповідність її ідентифікатора
        /// та типу, строк дії і токен підтвердження. Після успішної перевірки
        /// викликає передану операцію зміни стану користувача.
        ///
        /// Якщо операція виконана успішно, позначає дію підтвердженою,
        /// прибирає її з активного стану агрегату, оновлює час зміни
        /// користувача та записує зміну дочірньої сутності.
        ///
        /// Якщо перевірка або передана операція повертає помилку,
        /// подальші кроки підтвердження не виконуються.
        ///
        /// Доменну подію відповідного сценарію записує публічний метод.
        ///
        /// (Executes the shared pending-action confirmation process.
        ///
        /// Validates required arguments, chronological consistency,
        /// user access, action existence, identifier and type correspondence,
        /// expiration, and confirmation token. After successful validation,
        /// invokes the supplied user-state mutation.
        ///
        /// When the operation succeeds, marks the action as confirmed,
        /// removes it from the aggregate's active state, updates the user's
        /// modification timestamp, and records the child-entity change.
        ///
        /// If validation or the supplied operation returns a failure,
        /// the remaining confirmation steps are not performed.
        ///
        /// The scenario-specific public method records the domain event.)
        /// </summary>
        /// <typeparam name="TAction">
        /// Конкретний тип відкладеної дії, очікуваний поточним сценарієм.
        ///
        /// (Specific pending-action type expected by the current scenario.)
        /// </typeparam>
        /// <param name="confirmationToken">
        /// Токен підтвердження відкладеної дії.
        ///
        /// (Pending-action confirmation token.)
        /// </param>
        /// <param name="actionId">
        /// Очікуваний ідентифікатор поточної відкладеної дії.
        ///
        /// (Expected identifier of the current pending action.)
        /// </param>
        /// <param name="now">
        /// Час перевірок, застосування зміни та підтвердження дії.
        ///
        /// (Time used for validation, applying the change,
        /// and confirming the action.)
        /// </param>
        /// <param name="applyAction">
        /// Операція, яка застосовує відповідну зміну до стану користувача.
        /// Якщо повертає помилку, повинна залишати стан агрегату незмінним.
        ///
        /// (Operation that applies the corresponding change to user state.
        /// It must leave aggregate state unchanged when returning a failure.)
        /// </param>
        /// <returns>
        /// Успіх після застосування зміни та підтвердження дії;
        /// інакше — відповідна доменна помилка перевірки
        /// або переданої операції.
        ///
        /// (Success after applying the change and confirming the action;
        /// otherwise, the corresponding domain error from validation
        /// or the supplied operation.)
        /// </returns>
        private Result ConfirmPendingAction<TAction>(
            ConfirmationFlowToken confirmationToken,
            UserPendingActionId actionId,
            DateTimeOffset now,
            Func<TAction, Result> applyAction)
            where TAction : UserAction
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (confirmationToken, nameof(confirmationToken)),
                (actionId, nameof(actionId)),
                (applyAction, nameof(applyAction))
            );

            EnsureValidOperationTimestamp(now);

            var access = ProvideAccess(now);

            if (access.IsFailure)
            {
                return access;
            }

            var currentAction = _action;

            if (currentAction is null)
            {
                return Result.Failure(UserErrors.EmptyAction<User>());
            }

            if (currentAction.Id != actionId)
            {
                return Result.Failure(UserErrors.NotFoundAction<User>());
            }

            if (currentAction.UserAction is not TAction action)
            {
                return Result.Failure(UserErrors.InvalidActionType<User>());
            }

            var confirmValidation = currentAction.ValidateConfirmation(
                confirmationToken,
                now
            );

            if (confirmValidation.IsFailure)
            {
                return confirmValidation;
            }

            var applyResult = applyAction(action);

            if (applyResult.IsFailure)
            {
                return Result.Failure(applyResult.Errors);
            }

            currentAction.MarkAsConfirmed(
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
