using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;

namespace Authorization.Domain.UsersPendingRegistration.DomainEvents
{
    public class UserPendingRegistrationEvent : DomainEvent
    {
        public UserPendingRegistrationId Id { get; }

        public Login Login { get; }

        public UserPendingRegistrationEvent(UserPendingRegistrationId id, Login login, DateTimeOffset dateTime)
            : base(dateTime)
        {
            Id = id;
            Login = login;
        }
    }
}
