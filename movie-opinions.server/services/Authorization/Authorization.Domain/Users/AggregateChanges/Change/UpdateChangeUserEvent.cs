using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingChange;

namespace Authorization.Domain.Users.AggregateChanges.Change
{
    internal class UpdateChangeUserEvent : AggregateChange
    {
        public UserPendingChange UserPendingChange { get; }

        public DateTimeOffset Now { get; }

        public UpdateChangeUserEvent(UserPendingChange userPendingChange, DateTimeOffset now)
            : base(now)
        {
            UserPendingChange = userPendingChange;
            Now = now;
        }
    }
}
