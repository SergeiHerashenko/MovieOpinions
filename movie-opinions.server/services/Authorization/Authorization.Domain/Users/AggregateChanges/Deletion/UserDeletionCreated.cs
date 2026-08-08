using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersDeletion;

namespace Authorization.Domain.Users.AggregateChanges.Deletion
{
    public sealed class UserDeletionCreated : AggregateChange
    {
        public UserDeletion UserDeletion { get; }

        public UserDeletionCreated(UserDeletion userDeletion, DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserDeletion = userDeletion;
        }
    }
}
