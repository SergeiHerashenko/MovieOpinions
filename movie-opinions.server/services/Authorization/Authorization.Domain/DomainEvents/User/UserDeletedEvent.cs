namespace Authorization.Domain.DomainEvents.User
{
    public class UserDeletedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public DateTime DeleteTime { get; }

        public UserDeletedEvent(Guid userId, DateTime deleteTime)
            : base(deleteTime)
        {
            UserId = userId;
            DeleteTime = deleteTime;
        }
    }
}
