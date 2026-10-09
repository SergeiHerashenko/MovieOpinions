using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.ValueObjects.LoginUser;

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
        }

        public Result RequestPasswordChange()
        {

        }

        public Result RequestAccountDeletion()
        {

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

            }

            public override void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {

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

            }

            public override void RecordEventsAndChanges(
                User user,
                DateTimeOffset now)
            {

            }
        }
    }
}
