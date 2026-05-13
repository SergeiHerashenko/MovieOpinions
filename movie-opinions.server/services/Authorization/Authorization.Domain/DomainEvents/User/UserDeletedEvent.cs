namespace Authorization.Domain.DomainEvents.User
{
    public class UserDeletedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public DateTimeOffset DeleteTime { get; }

        public UserDeletedEvent(Guid userId, DateTimeOffset deleteTime)
            : base(deleteTime)
        {
            UserId = userId;
            DeleteTime = deleteTime;
        }
    }
}
