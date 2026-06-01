using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersDeletion.ValueObjects;

namespace Authorization.Domain.UsersDeletion
{
    public class UserDeletion : AggregateRoot<UserDeletionId, Guid>
    {
        public UserDeletionId UserDeletionId { get; }

        public UserId UserId { get; }

        public Login Login { get; }

        public string? Reason { get; }

        public DateTimeOffset RestoreUntil;

        public DateTimeOffset? RestoredAt;

        public DeletionStatus DeletionStatus { get; }
    }
}
