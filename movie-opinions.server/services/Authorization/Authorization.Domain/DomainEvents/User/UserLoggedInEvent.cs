namespace Authorization.Domain.DomainEvents.User
{
    public class UserLoggedInEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string IpAddress { get; }

        public DateTime LoginTime { get; }

        public UserLoggedInEvent(Guid userId, string ipAddress, DateTime loginTime)
            : base(loginTime)
        {
            UserId = userId;
            IpAddress = ipAddress;
            LoginTime = loginTime;
        }
    }
}
