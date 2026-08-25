using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Action;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        public Result ConfirmLogin(
            string confirmationToken,
            UserPendingActionId actionId,
            DateTimeOffset now)
        {
            return CompleteAction<LoginChangeAction>(
                confirmationToken,
                actionId,
                now,
                change =>
                {
                    Login = change.NewLogin;
                    IsLoginConfirmed = true;

                    return Result.Success();
                }
            );
        }

        public Result ConfirmPassword(
            string confirmationToken,
            UserPendingActionId actionId,
            DateTimeOffset now)
        {
            return CompleteAction<PasswordChangeAction>(
                confirmationToken,
                actionId,
                now,
                change =>
                {
                    Password = change.NewPassword;

                    return Result.Success();
                }
            );
        }

        public Result ConfirmDeleting(
            string confirmationToken,
            UserPendingActionId actionId,
            DateTimeOffset now)
        {
            return CompleteAction<DeleteAccountAction>(
                confirmationToken,
                actionId,
                now,
                change =>
                {
                    return Delete(now, change.Reason);
                }
            );
        }

        private Result CompleteAction<TAction>(
            string confirmationToken,
            UserPendingActionId actionId,
            DateTimeOffset now,
            Func<TAction, Result> applyAction)
            where TAction : UserAction
        {
            var access = ProvideAccess();

            if (access.IsFailure)
                return access;

            if (_action is null)
                return Result.Failure(ActionErrors.EmptyAction<User>());

            if (_action.Id != actionId)
                return Result.Failure(ActionErrors.InvalidUserPendingActionId<User>());

            var action = _action.UserAction as TAction;

            if (action is null)
                return Result.Failure(ActionErrors.InvalidActionType<User>());

            var confirmResult = _action.ConfirmAction(confirmationToken, now);

            if (confirmResult.IsFailure)
                return confirmResult;

            var applyResult = applyAction(action);

            if(applyResult.IsFailure)
                return Result.Failure(applyResult.Errors);

            UpdatedAt = now;

            var actionChange = _action;

            _action = null;

            AddAggregateChange(new UserPendingActionUpdated(
                actionChange, 
                now)
            );

            AddDomainEvent(new UserActionEvent(
                actionChange.Id,
                Login,
                actionChange.UserAction,
                actionChange.ExpiresAt,
                now)
            );

            return Result.Success();
        }
    }
}
