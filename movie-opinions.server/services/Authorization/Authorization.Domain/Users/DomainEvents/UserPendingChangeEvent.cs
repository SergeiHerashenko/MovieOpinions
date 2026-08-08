using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingChange.Changes;
using Authorization.Domain.Users.Entities.UsersPendingChange.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserPendingChangeEvent : DomainEvent
    {
        public UserPendingChangeId UserPendingChangeId { get; }

        public UserChange UserChange { get; }

        public DateTimeOffset ExpiresAt { get; }

        public DateTimeOffset Now { get; }

        public UserPendingChangeEvent(UserPendingChangeId userPendingChangeId, UserChange userChange, DateTimeOffset expiresAt, DateTimeOffset now)
            : base(now)
        {
            UserPendingChangeId = userPendingChangeId;
            UserChange = userChange;
            ExpiresAt = expiresAt;
            Now = now;
        }
    }
}
