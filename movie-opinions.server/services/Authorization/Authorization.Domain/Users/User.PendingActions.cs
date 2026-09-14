using Authorization.Domain.Common.Errors.Common;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Action;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        public Result<UserPendingAction> ActionChangeLogin(Login newLogin, DateTimeOffset now)
        {
            if (newLogin is null)
                return Result<UserPendingAction>.Failure(LoginErrors.EmptyLogin<User>());

            var access = ProvideAccess();

            if (access.IsFailure)
                return Result<UserPendingAction>.Failure(access.Errors);

            ExpirePendingActionIfNeeded(now);

            if (_action is not null)
                return Result<UserPendingAction>.Failure(ActionErrors.ActionAlreadyExists<User>());

            if (newLogin == Login)
                return Result<UserPendingAction>.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(Login)));

            var actionChengeLogin = UserAction.From(newLogin);

            var createActionResult = CreateAction(actionChengeLogin, now);

            if (createActionResult.IsFailure)
                return createActionResult;

            var createdAction = createActionResult.Value;

            AddAggregateChange(new UserPendingActionCreated(createdAction, now));

            return Result<UserPendingAction>.Success(createdAction);
        }

        public Result<UserPendingAction> ActionChangePassword(
            PlainPassword plainNewPassword,
            Password newPassword,
            Func<PlainPassword, string, bool> verifier,
            DateTimeOffset now)
        {
            if (verifier is null)
                throw DomainInvalidOperationException.NullCallback<User>(
                    nameof(verifier),
                    OperationType.Compare
                );

            if (newPassword is null)
                return Result<UserPendingAction>.Failure(PasswordErrors.EmptyHashPassword<User>());

            if(plainNewPassword is null)
                return Result<UserPendingAction>.Failure(PasswordErrors.EmptyPlainPassword<User>());

            var access = ProvideAccess();

            if (access.IsFailure)
                return Result<UserPendingAction>.Failure(access.Errors);

            ExpirePendingActionIfNeeded(now);

            if (_action is not null)
                return Result<UserPendingAction>.Failure(ActionErrors.ActionAlreadyExists<User>());

            var passwordIsMatch = Password.Matches(plainNewPassword, verifier);

            if (passwordIsMatch.IsFailure)
                return Result<UserPendingAction>.Failure(passwordIsMatch.Errors);

            if(passwordIsMatch.Value)
                return Result<UserPendingAction>.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(Password)));

            var actionChangePassword = UserAction.From(newPassword);

            var createActionResult = CreateAction(actionChangePassword, now);

            if (createActionResult.IsFailure)
                return createActionResult;

            var createdAction = createActionResult.Value;

            AddAggregateChange(new UserPendingActionCreated(createdAction, now));

            return Result<UserPendingAction>.Success(createdAction);
        }

        public Result<UserPendingAction> ActionDeletingUser(
            string? reason, 
            DateTimeOffset now)
        {
            var access = ProvideAccess();

            if (access.IsFailure)
                return Result<UserPendingAction>.Failure(access.Errors);

            var deletionReason = DeletionReason.Create(reason);

            if (deletionReason.IsFailure)
                return Result<UserPendingAction>.Failure(deletionReason.Errors);

            ExpirePendingActionIfNeeded(now);

            if (_action is not null)
                return Result<UserPendingAction>.Failure(ActionErrors.ActionAlreadyExists<User>());

            var actionDeletingUser = UserAction.From(deletionReason.Value);

            var createActionDeletingResult = CreateAction(actionDeletingUser, now);

            if (createActionDeletingResult.IsFailure)
                return createActionDeletingResult;

            var createdAction = createActionDeletingResult.Value;

            AddAggregateChange(new UserPendingActionCreated(createActionDeletingResult.Value, now));

            return Result<UserPendingAction>.Success(createdAction);
        }

        public Result FailPendingAction(UserPendingActionId userPendingActionId, DateTimeOffset now)
        {
            if (_action is null)
                return Result.Failure(DeletionErrors.NotDeleteUser<User>());

            if (_action.Id != userPendingActionId)
                return Result.Failure(DeletionErrors.NotFoundAction<User>());

            var failResult = _action.MarkAsFailed(userPendingActionId);

            if (failResult.IsFailure)
                return failResult;

            AddAggregateChange(new UserPendingActionUpdated(_action, now));

            _action = null;

            return Result.Success();
        }

        public Result<UserPendingAction> GetPendingAction()
        {
            if (_action is null)
                return Result<UserPendingAction>.Failure(ActionErrors.EmptyAction<User>());

            return Result<UserPendingAction>.Success(_action);
        }

        public Result<UserPendingAction> GetActionForConfirmation<TAction>(ConfirmationFlowToken confirmationToken, DateTimeOffset now)
        {
            var access = ProvideAccess();

            if (access.IsFailure)
                return Result<UserPendingAction>.Failure(access.Errors);

            if (_action is null)
                return Result<UserPendingAction>.Failure(ActionErrors.EmptyAction<User>());

            if (_action.ExpiresAt <= now)
                return Result<UserPendingAction>.Failure(ActionErrors.Expired<User>());

            if (_action.ConfirmationToken != confirmationToken)
                return Result<UserPendingAction>.Failure(ActionErrors.InvalidConfirmationToken<User>());

            if (_action.UserAction is not TAction)
                return Result<UserPendingAction>.Failure(ActionErrors.InvalidActionType<User>());

            return Result<UserPendingAction>.Success(_action);
        }

        private void ExpirePendingActionIfNeeded(DateTimeOffset now)
        {
            if (_action is null)
                return;

            if (_action.ExpiresAt > now)
                return;

            var expiredAction = _action;

            expiredAction.MarkAsExpired(now);

            AddAggregateChange(new UserPendingActionUpdated(expiredAction, now));

            _action = null;
        }

        private Result<UserPendingAction> CreateAction(UserAction userChange, DateTimeOffset now)
        {
            var actionResult = UserPendingAction.Create(Id, userChange, now);

            if (actionResult.IsFailure)
                return actionResult;

            _action = actionResult.Value;

            AddDomainEvent(new UserPendingActionEvent(
                actionResult.Value.Id,
                Login,
                actionResult.Value.UserAction,
                actionResult.Value.ExpiresAt,
                now)
            );

            return Result<UserPendingAction>.Success(actionResult.Value);
        }
    }
}
