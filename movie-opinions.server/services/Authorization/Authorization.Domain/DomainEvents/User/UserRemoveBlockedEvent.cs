using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.DomainEvents.User
{
    public class UserRemoveBlockedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public UserRemoveBlockedEvent(UserId userId, Login login, DateTimeOffset dateTime)
            : base(dateTime)
        {
            UserId = userId;
            Login = login;
        }
    }
}
