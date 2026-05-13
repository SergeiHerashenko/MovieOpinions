namespace Authorization.Domain.DomainEvents.User
{
    public class UserLoggedInEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string IpAddress { get; }

        public DateTimeOffset LoginTime { get; }

        public UserLoggedInEvent(Guid userId, string ipAddress, DateTimeOffset loginTime)
            : base(loginTime)
        {
            UserId = userId;
            IpAddress = ipAddress;
            LoginTime = loginTime;
        }
    }
}
