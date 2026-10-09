using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserRegisteredEvent : DomainEvent
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public UserRegisteredEvent(
            UserId userId,
            Login login,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserId = userId;
            Login = login;
        }
    }
}
