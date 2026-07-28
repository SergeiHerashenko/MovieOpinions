using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserUndeleteEvent : DomainEvent
    {
        public Login Login { get; }

        public DateTimeOffset UndeleteTime { get; }

        public UserUndeleteEvent(Login login, DateTimeOffset undeleteTime)
            : base(undeleteTime)
        {
            Login = login;
            UndeleteTime = undeleteTime;
        }
    }
}
