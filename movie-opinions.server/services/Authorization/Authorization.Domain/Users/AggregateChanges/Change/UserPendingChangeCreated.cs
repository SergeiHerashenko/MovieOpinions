using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingChange;

namespace Authorization.Domain.Users.AggregateChanges.Change
{
    public class UserPendingChangeCreated : AggregateChange
    {
        public UserPendingChange UserPendingChange { get; }

        public DateTimeOffset Now { get; }

        public UserPendingChangeCreated(UserPendingChange userPendingChange, DateTimeOffset now)
            : base(now)
        {
            UserPendingChange = userPendingChange;
            Now = now;
        }
    }
}
