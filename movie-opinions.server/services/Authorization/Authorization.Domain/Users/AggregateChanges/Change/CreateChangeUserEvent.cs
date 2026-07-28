using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingChange;

namespace Authorization.Domain.Users.AggregateChanges.Change
{
    public class CreateChangeUserEvent : AggregateChange
    {
        public UserPendingChange UserPendingChange { get; }

        public DateTimeOffset Now { get; }

        public CreateChangeUserEvent(UserPendingChange userPendingChange, DateTimeOffset now)
            : base(now)
        {
            UserPendingChange = userPendingChange;
            Now = now;
        }
    }
}
