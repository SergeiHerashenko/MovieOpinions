using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersDeletion;

namespace Authorization.Domain.Users.AggregateChanges.Deletion
{
    public sealed class UserDeletionUpdated : AggregateChange
    {
        public UserDeletion UserDeletion { get; }

        public UserDeletionUpdated(UserDeletion userDeletion, DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserDeletion = userDeletion;
        }
    }
}
