using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction;

namespace Authorization.Domain.Users.AggregateChanges.Restriction
{
    public sealed class UserRestrictionUpdated : AggregateChange
    {
        public UserRestriction UserRestriction { get; }

        public UserRestrictionUpdated(
            UserRestriction userRestriction,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestriction = userRestriction;
        }
    }
}
