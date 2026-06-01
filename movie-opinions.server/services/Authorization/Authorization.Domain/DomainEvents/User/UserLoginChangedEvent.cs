using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.DomainEvents.User
{
    public class UserLoginChangedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public string OldLogin { get; }

        public Login NewLogin { get; }

        public UserLoginChangedEvent(UserId userId, string oldLogin, Login newLogin, DateTimeOffset dateTime)
            : base(dateTime)
        {
            UserId = userId;
            OldLogin = oldLogin;
            NewLogin = newLogin;
        }
    }
}
