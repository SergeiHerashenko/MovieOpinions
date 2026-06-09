using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;

namespace Authorization.Domain.DomainEvents.UserPendingRegistration
{
    public class UserPendingRegistrationRequestedEvent : DomainEvent
    {
        public UserPendingRegistrationId UserPendingRegistrationId { get; }

        public Login Login { get; }

        public UserPendingRegistrationRequestedEvent(UserPendingRegistrationId userPendingRegistrationId, Login login, DateTimeOffset dateTime)
            : base(dateTime)
        {
            UserPendingRegistrationId = userPendingRegistrationId;
            Login = login;
        }
    }
}
