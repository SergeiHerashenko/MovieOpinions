using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserDeletedEvent : DomainEvent
    {
        public UserDeletionId UserDeletionId { get; }

        public Login Login { get; }

        public DateTimeOffset DeletedAt { get; }

        public UserDeletedEvent(UserDeletionId userDeletionId, Login login, DateTimeOffset deletedAt)
            : base(deletedAt)
        {
            UserDeletionId = userDeletionId;
            Login = login;
            DeletedAt = deletedAt;
        }
    }
}
