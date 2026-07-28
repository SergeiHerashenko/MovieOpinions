using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction;

namespace Authorization.Domain.Users.AggregateChanges.Restriction
{
    public sealed class CreateRestrictionUserEvent : AggregateChange
    {
        public UserRestriction Restriction { get; }

        public CreateRestrictionUserEvent(
            UserRestriction restriction,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            Restriction = restriction;
        }
    }
}
