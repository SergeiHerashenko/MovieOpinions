using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserDeletionEvent : DomainEvent
    {
        public Login Login { get; }

        public DateTimeOffset DeletedAt { get; }

        public UserDeletionEvent(Login login, DateTimeOffset deletedAt)
            : base(deletedAt)
        {
            Login = login;
            DeletedAt = deletedAt;
        }
    }
}
