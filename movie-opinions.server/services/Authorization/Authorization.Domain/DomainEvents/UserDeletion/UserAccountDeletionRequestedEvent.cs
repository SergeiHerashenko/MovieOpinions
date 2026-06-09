using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.DomainEvents.UserDeletion
{
    public class UserAccountDeletionRequestedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public string? Reason { get; }

        public DateTimeOffset RestoreUntil { get; }

        public UserAccountDeletionRequestedEvent(UserId userId, Login login, string? reason, DateTimeOffset restoreUntil, DateTimeOffset dateTime)
            : base(dateTime)
        {
            UserId = userId;
            Login = login;
            Reason = reason;
            RestoreUntil = restoreUntil;
        }
    }
}
