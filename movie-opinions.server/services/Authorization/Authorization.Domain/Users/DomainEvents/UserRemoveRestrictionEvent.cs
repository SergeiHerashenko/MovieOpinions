using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserRemoveRestrictionEvent : DomainEvent
    {
        public RestrictionType RestrictionType { get; }

        public RestrictionRule RestrictionRule { get; }

        public DateTimeOffset RestrictionCanselDate { get; }

        public UserRemoveRestrictionEvent(RestrictionType restrictionType, RestrictionRule restrictionRule, DateTimeOffset restrictionCanselDate)
            : base(restrictionCanselDate)
        {
            RestrictionType = restrictionType;
            RestrictionRule = restrictionRule;
            RestrictionCanselDate = restrictionCanselDate;
        }
    }
}
