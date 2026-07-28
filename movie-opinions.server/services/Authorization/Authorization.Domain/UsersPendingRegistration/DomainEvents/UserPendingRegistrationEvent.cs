using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.UsersPendingRegistration.DomainEvents
{
    public class UserPendingRegistrationEvent : DomainEvent
    {
        public Login Login { get; }

        public UserPendingRegistrationEvent(Login login, DateTimeOffset dateTime)
            : base(dateTime)
        {
            Login = login;
        }
    }
}
