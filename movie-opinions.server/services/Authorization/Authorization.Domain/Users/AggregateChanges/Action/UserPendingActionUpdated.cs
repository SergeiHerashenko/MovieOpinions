using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingAction;

namespace Authorization.Domain.Users.AggregateChanges.Action
{
    public class UserPendingActionUpdated : AggregateChange
    {
        public UserPendingAction UserPendingAction { get; }

        public DateTimeOffset Now { get; }

        public UserPendingActionUpdated(UserPendingAction userPendingAction, DateTimeOffset now)
            : base(now)
        {
            UserPendingAction = userPendingAction;
            Now = now;
        }
    }
}
