using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserRestrictionSessionRemovedRestrictionEvent : DomainEvent
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        public Login Login { get; }

        public RestrictionRule RestrictionRule { get; }

        public RestrictionType RestrictionType { get; }

        public int TotalBlockedMinutes { get; }

        public UserRestrictionSessionRemovedRestrictionEvent(
            UserRestrictionSessionId userRestrictionSessionId,
            Login login,
            RestrictionRule restrictionRule,
            RestrictionType restrictionType,
            int totalBlockedMinutes,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSessionId = userRestrictionSessionId;
            Login = login;
            RestrictionRule = restrictionRule;
            RestrictionType = restrictionType;
            TotalBlockedMinutes = totalBlockedMinutes;
        }
    }
}
