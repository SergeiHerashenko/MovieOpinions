using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction;

namespace Authorization.Domain.Users.AggregateChanges.Restriction
{
    public sealed class UpdateRestrictionUserEvent : AggregateChange
    {
        public UserRestriction UserRestriction { get; }

        public UpdateRestrictionUserEvent(
            UserRestriction userRestriction,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestriction = userRestriction;
        }
    }
}
