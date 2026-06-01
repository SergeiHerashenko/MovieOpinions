using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.DomainEvents.User
{
    public class UserPasswordChangedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public UserPasswordChangedEvent(UserId userId, Login login, DateTimeOffset dateTime)
            : base(dateTime)
        {
            UserId = userId; 
            Login = login;
        }
    }
}
