using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingChange.Changes;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserPendingChangeEvent : DomainEvent
    {
        public UserId UserId { get; }

        public UserChange UserChange { get; }

        public DateTimeOffset ExpiresAt { get; }

        public DateTimeOffset Now { get; }

        public UserPendingChangeEvent(UserId userId, UserChange userChange, DateTimeOffset expiresAt, DateTimeOffset now)
            : base(now)
        {
            UserId = userId;
            UserChange = userChange;
            ExpiresAt = expiresAt;
            Now = now;
        }
    }
}
