using Authorization.Domain.Enums;

namespace Authorization.Domain.DomainEvents.User
{
    public class UserRegisteredEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string Login { get; }

        public LoginType LoginType { get; }

        public UserRegisteredEvent(Guid userId, string login, LoginType loginType, DateTime dateTime)
            : base(dateTime)
        {
            UserId = userId;
            Login = login;
            LoginType = loginType;
        }
    }
}
