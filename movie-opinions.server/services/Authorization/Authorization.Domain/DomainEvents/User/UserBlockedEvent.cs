namespace Authorization.Domain.DomainEvents.User
{
    public class UserBlockedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string Reason { get; }

        public UserBlockedEvent(Guid userId, string reason, DateTime dateTime)
            : base(dateTime)
        {
            UserId = userId;
            Reason = reason;
        }
    }
}
