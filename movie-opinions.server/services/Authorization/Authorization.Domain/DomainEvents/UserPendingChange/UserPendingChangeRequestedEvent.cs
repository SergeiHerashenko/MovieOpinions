using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersPendingChange.Changes;
using Authorization.Domain.UsersPendingChange.ValueObjects;

namespace Authorization.Domain.DomainEvents.UserPendingChange
{
    public class UserPendingChangeRequestedEvent : DomainEvent
    {
        public UserPendingChangeId UserPendingChangeId { get; }

        public UserId UserId { get; }

        public ConfirmationToken ConfirmationToken { get; }

        public UserChange UserChange { get; }

        public DateTimeOffset ExpiresAt { get; }

        public UserPendingChangeRequestedEvent(
            UserPendingChangeId userPendingChangeId,
            UserId userId,
            ConfirmationToken confirmationToken,
            UserChange userChange,
            DateTimeOffset expiresAt,
            DateTimeOffset dateTime)
            : base(dateTime)
        {
            UserPendingChangeId = userPendingChangeId;
            UserId = userId;
            ConfirmationToken = confirmationToken;
            UserChange = userChange;
            ExpiresAt = expiresAt;
        }
    }
}
