using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.DomainEvents.User
{
    public class UserLoggedInEvent : DomainEvent
    {
        public Guid UserId { get; }

        public Login Login { get; }

        public string IpAddress { get; }

        public DateTimeOffset LoginTime { get; }

        public UserLoggedInEvent(Guid userId, Login login, string ipAddress, DateTimeOffset loginTime)
            : base(loginTime)
        {
            UserId = userId;
            Login = login;
            IpAddress = ipAddress;
            LoginTime = loginTime;
        }
    }
}
