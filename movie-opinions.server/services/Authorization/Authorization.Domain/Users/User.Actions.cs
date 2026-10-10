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
        public Result RequestLoginChange(
            Login newLogin,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (newLogin, nameof(newLogin))
            );

            if (newLogin == Login)
                return Result.Failure(UserErrors.NoUpdateNeeded<User>());

            var userAction = UserAction.From(newLogin);

            return RequestPendingAction(
                userAction,
                now
            );
        }

        public Result RequestPasswordChange(
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

            var passwordIsMatch = Password.Matches(plainNewPassword, verifier);

            if (passwordIsMatch)
                return Result.Failure(UserErrors.NoUpdateNeeded<User>());

            var userAction = UserAction.From(newPassword);

            return RequestPendingAction(
                userAction,
                now
            );
        }

        public Result RequestAccountDeletion(
            string? reason,
            DateTimeOffset now)
        {
            var deletionReason = DeletionReason.Create(reason);

            if (deletionReason.IsFailure)
                return Result.Failure(deletionReason.Errors);

            var userAction = UserAction.From(deletionReason.Value);

            return RequestPendingAction(
                userAction,
                now
            );
        }

        private Result RequestPendingAction(
            UserAction userAction,
            DateTimeOffset now)
        {
            var planResult = PreparePendingActionRequestPlan(
                userAction,
                now
            );

            if (planResult.IsFailure)
                return Result.Failure(planResult.Errors);

            var plan = planResult.Value;

            plan.Apply(
                this,
                now
            );

            plan.RecordEventsAndChanges(
                this,
                now
            );

            return Result.Success();
        }

        private abstract class PendingActionRequestPlan
        {
            public UserPendingAction Action { get; }

            protected PendingActionRequestPlan(UserPendingAction action)
            {
                Action = action;
            }

            public abstract void Apply(
                User user,
                DateTimeOffset now);

            public abstract void RecordEventsAndChanges(
                User user,
                DateTimeOffset now);
        }

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

            public override void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddDomainEvent(new UserPendingActionCreatedEvent(
                    Action.Id,
                    user.Login,
                    Action.UserAction,
                    Action.ExpiresAt,
                    now)
                );

                user.AddAggregateChange(new UserPendingActionCreated(
                    Action,
                    now)
                );
            }
        }

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

            public override void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {
                user.AddDomainEvent(new UserPendingActionCreatedEvent(
                    Action.Id,
                    user.Login,
                    Action.UserAction,
                    Action.ExpiresAt,
                    now)
                );

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

        private Result<PendingActionRequestPlan> PreparePendingActionRequestPlan(
            UserAction userAction,
            DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<User>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            var access = ProvideAccess(now);

            if (access.IsFailure)
                return Result<PendingActionRequestPlan>.Failure(access.Errors);

            if(_action is null)
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

            if (_action.IsActive(now))
                return Result<PendingActionRequestPlan>.Failure(UserErrors.ActionAlreadyExists<User>());

            _action.ValidateCanExpire(now);

            var replacementAction = UserPendingAction.Create(
                Id,
                userAction,
                now
            );

            return Result<PendingActionRequestPlan>.Success(
                new ReplaceExpiredPendingActionPlan(
                    replacementAction,
                    _action
                )
            );
        }
    }
}
