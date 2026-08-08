using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserRestrictionRemovedEvent : DomainEvent
    {
        public UserRestrictionId UserRestrictionId { get; }

        public RestrictionType RestrictionType { get; }

        public RestrictionRule RestrictionRule { get; }

        public DateTimeOffset RestrictionCanselDate { get; }

        public UserRestrictionRemovedEvent(UserRestrictionId userRestrictionId, RestrictionType restrictionType, RestrictionRule restrictionRule, DateTimeOffset restrictionCanselDate)
            : base(restrictionCanselDate)
        {
            UserRestrictionId = userRestrictionId;
            RestrictionType = restrictionType;
            RestrictionRule = restrictionRule;
            RestrictionCanselDate = restrictionCanselDate;
        }
    }
}
