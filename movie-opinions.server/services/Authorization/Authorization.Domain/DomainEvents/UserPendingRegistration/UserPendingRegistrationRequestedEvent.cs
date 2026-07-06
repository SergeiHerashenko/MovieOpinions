using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.DomainEvents.UserPendingRegistration
{
    public class UserPendingRegistrationRequestedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public UserPendingRegistrationRequestedEvent(UserId userId, Login login, DateTimeOffset dateTime)
            : base(dateTime)
        {
            UserId = userId;
            Login = login;
        }
    }
}
