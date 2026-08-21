using Authorization.Domain.Common.Errors.Common;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Entities.UsersPendingAction
{
    public class UserPendingAction : Entity<UserPendingActionId>
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromMinutes(30);

        public UserId UserId { get; private set; }

        public ConfirmationToken ConfirmationToken { get; private set; }

        public UserAction UserAction { get; private set; }

        public DateTimeOffset ExpiresAt { get; private set; }

        public DateTimeOffset? ConfirmationTime { get; private set; }

        public DateTimeOffset? ExpiredAt { get; private set; }

        public DateTimeOffset? CancelledTime { get; private set; }

        public ActionStatus Status { get; private set; }

        #region Creation
        private UserPendingAction(
            UserPendingActionId userPendingActionId, 
            UserId userId, 
            ConfirmationToken confirmToken, 
            UserAction userAction,
            DateTimeOffset now)
            : base(userPendingActionId, now)
        {
            UserId = userId;
            ConfirmationToken = confirmToken;
            UserAction = userAction;
            ExpiresAt = now.Add(ExpirationTime);
            ConfirmationTime = null;
            ExpiredAt = null;
            CancelledTime = null;
            Status = ActionStatus.Active;
        }

        internal static Result<UserPendingAction> Create(UserId userId, UserAction userAction, DateTimeOffset now)
        {
            if (userId is null)
                return Result<UserPendingAction>.Failure(CommonErrors.Identifier.EmptyIdentifier<UserPendingAction>(nameof(userId)));

            if (userAction is null)
                return Result<UserPendingAction>.Failure(ActionErrors.EmptyAction<UserPendingAction>());

            var pendingAction = new UserPendingAction(
                UserPendingActionId.Create(),
                userId,
                ConfirmationToken.Create(),
                userAction,
                now
            );

            return Result<UserPendingAction>.Success(pendingAction);
        }
        #endregion

        #region Restoration
        private UserPendingAction(
            UserPendingActionId userPendingActionId,
            UserId userId,
            ConfirmationToken confirmToken,
            UserAction userAction,
            DateTimeOffset expiresAt,
            DateTimeOffset? confirmationTime,
            DateTimeOffset? expiredAt,
            DateTimeOffset? cancelledTime,
            ActionStatus changeStatus,
            DateTimeOffset createdAt)
            : base(userPendingActionId, createdAt)
        {
            UserId = userId;
            ConfirmationToken = confirmToken;
            UserAction = userAction;
            ExpiresAt = expiresAt;
            ConfirmationTime = confirmationTime;
            ExpiredAt = expiredAt;
            CancelledTime = cancelledTime;
            Status = changeStatus;
        }

        public static UserPendingAction Restore(
            UserPendingActionId userPendingActionId,
            UserId userId,
            ConfirmationToken confirmationToken,
            UserAction userAction,
            DateTimeOffset expiresAt,
            DateTimeOffset? confirmationTime,
            DateTimeOffset? expiredAt,
            DateTimeOffset? cancelledTime,
            ActionStatus changeStatus,
            DateTimeOffset createdAt)
        {
            DomainGuard.AgainstNull<UserPendingAction>(
                (userPendingActionId, nameof(userPendingActionId)),
                (userId, nameof(userId)),
                (confirmationToken, nameof(confirmationToken)),
                (userAction, nameof(userAction))
            );

            ValidateState(userPendingActionId, changeStatus, expiresAt, confirmationTime, expiredAt, createdAt);

            return new UserPendingAction(userPendingActionId, userId, confirmationToken, userAction, expiresAt, confirmationTime, expiredAt, cancelledTime, changeStatus, createdAt);
        }
        #endregion

        #region Behavior
        internal Result ConfirmAction(string confirmationToken, DateTimeOffset now)
        {
            if (Status == ActionStatus.Confirmed)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<UserPendingAction>(nameof(Status)));

            if (Status == ActionStatus.Expired)
                return Result.Failure(ActionErrors.Expired<UserPendingAction>());

            if (Status == ActionStatus.Cancelled)
                return Result.Failure(CommonErrors.StateConflict.ActionCancelled<UserPendingAction>(UserAction.ActionType));

            if (ExpiresAt <= now)
            {
                Status = ActionStatus.Expired;
                ExpiredAt = now;

                return Result.Failure(ActionErrors.Expired<UserPendingAction>());
            }

            if (ConfirmationToken.Value != confirmationToken)
                return Result.Failure(ActionErrors.InvalidConfirmationToken<UserPendingAction>());

            Status = ActionStatus.Confirmed;
            ConfirmationTime = now;

            return Result.Success();
        }

        internal Result MarkAsFailed(UserPendingActionId userPendingActionId)
        {
            if (Status == ActionStatus.Failed)
            {
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<UserPendingAction>(nameof(Status)));
            }

            if (Status != ActionStatus.Active)
            {
                return Result.Failure(ActionErrors.InvalidStatusTransition<UserPendingAction>(Status, ActionStatus.Failed));
            }

            Status = ActionStatus.Failed;

            return Result.Success();
        }

        internal void MarkAsExpired(DateTimeOffset now)
        {
            if (Status == ActionStatus.Expired)
                return;

            if (now <= ExpiresAt)
                return;

            Status = ActionStatus.Expired;
            ExpiredAt = now;
        }
        #endregion

        #region Guard
        private static void ValidateState(
            UserPendingActionId userPendingActionId,
            ActionStatus actionStatus,
            DateTimeOffset expiresAt,
            DateTimeOffset? confirmationTime,
            DateTimeOffset? expiredAt,
            DateTimeOffset createdAt)
        {
            if (!Enum.IsDefined(actionStatus))
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<UserPendingAction>(nameof(actionStatus), actionStatus.ToString());

            if (expiresAt <= createdAt)
                throw DomainInvariantViolationException.BrokenState<UserPendingAction>(
                    $"End time '{nameof(expiresAt)}' cannot be less than creation time '{nameof(createdAt)}'!",
                    new Dictionary<string, object?>
                    {
                        ["expiresAt"] = expiresAt,
                        ["createdAt"] = createdAt
                    }
                );

            bool isValidState = (actionStatus, confirmationTime, expiredAt) switch
            {
                (ActionStatus.Active, null, null) => true,
                (ActionStatus.Confirmed, not null, null) => true,
                (ActionStatus.Expired, null, not null) => true,
                _ => false
            };

            if (!isValidState)
            {
                throw DomainInvariantViolationException.BrokenState<UserPendingAction>(
                    $"Dates conflict. ConfirmationTime: {confirmationTime?.ToString() ?? "null"}, ExpiredAt: {expiredAt?.ToString() ?? "null"}",
                    new Dictionary<string, object?>
                    {
                        ["Id"] = userPendingActionId,
                        ["ActionStatus"] = actionStatus,
                        ["ConfirmationTime"] = confirmationTime,
                        ["ExpiredAt"] = expiredAt
                    }
                );
            }
        }
        #endregion
    }
}
