namespace Authorization.Domain.DomainEvents.User
{
    public class UserPasswordChangedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public UserPasswordChangedEvent(Guid userId, DateTime dateTime)
            : base(dateTime)
        {
            UserId = userId;
        }
    }
}
