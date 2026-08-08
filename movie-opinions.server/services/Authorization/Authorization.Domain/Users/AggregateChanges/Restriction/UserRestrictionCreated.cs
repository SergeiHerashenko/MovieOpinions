using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction;

namespace Authorization.Domain.Users.AggregateChanges.Restriction
{
    public sealed class UserRestrictionCreated : AggregateChange
    {
        public UserRestriction Restriction { get; }

        public UserRestrictionCreated(
            UserRestriction restriction,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            Restriction = restriction;
        }
    }
}
