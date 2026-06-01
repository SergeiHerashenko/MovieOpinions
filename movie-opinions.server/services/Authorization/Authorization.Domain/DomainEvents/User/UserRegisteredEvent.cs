using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.DomainEvents.User
{
    public class UserRegisteredEvent : DomainEvent
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public UserRegisteredEvent(UserId userId, Login login, DateTimeOffset dateTime)
            : base(dateTime)
        {
            UserId = userId;
            Login = login;
        }
    }
}
