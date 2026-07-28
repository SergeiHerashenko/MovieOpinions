using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserRestrictionEvent : DomainEvent
    {
        public UserRestriction Restriction { get; }

        public UserRestrictionEvent(
            UserRestriction restriction,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            Restriction = restriction;
        }
    }
}
