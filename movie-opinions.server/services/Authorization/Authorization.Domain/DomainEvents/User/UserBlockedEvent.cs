using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.DomainEvents.User
{
    public class UserBlockedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public string Reason { get; }

        public UserBlockedEvent(UserId userId, Login login, string reason, DateTimeOffset dateTime) 
            : base(dateTime)
        {
            UserId = userId;
            Login = login;
            Reason = reason;
        }
    }
}
