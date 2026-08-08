using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserRestrictionEvent : DomainEvent
    {
        public UserRestrictionId UserRestrictionId { get; }

        public RestrictionRule RestrictionRule { get; }

        public RestrictionType RestrictionType { get; }

        public string? Reason { get; }

        public string RestrictedBy { get; }

        public UserRestrictionEvent(
            UserRestrictionId userRestrictionId,
            RestrictionRule restrictionRule,
            RestrictionType restrictionType,
            string? reason,
            string restrictedBy,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionId = userRestrictionId;
            RestrictionRule = restrictionRule;
            RestrictionType = restrictionType;
            Reason = reason;
            RestrictedBy = restrictedBy;
        }
    }
}
