namespace Authorization.Domain.DomainEvents.UserDeletion
{
    public class UserAccountDeletionRequestedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string Login { get; }

        public string? Reason { get; }

        public UserAccountDeletionRequestedEvent(Guid userId, string login, string? reason, DateTimeOffset deleteTime)
            : base(deleteTime)
        {
            UserId = userId;
            Login = login;
            Reason = reason;
        }
    }
}
