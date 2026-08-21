using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserRestrictionSessionAddRestrictionEvent : DomainEvent
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        public Login Login { get; }

        public IReadOnlyCollection<(RestrictionRule Rule, string? Reason)> RestrictionDescription { get; }

        public RestrictionType RestrictionType { get; }

        public int TotalBlockedMinutes { get; }

        public UserRestrictionSessionAddRestrictionEvent(
            UserRestrictionSessionId userRestrictionSessionId,
            Login login,
            IReadOnlyCollection<(RestrictionRule Rule, string? Reason)> restrictionDescription,
            RestrictionType restrictionType,
            int totalBlockedMinutes,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSessionId = userRestrictionSessionId;
            Login = login;
            RestrictionDescription = restrictionDescription;
            RestrictionType = restrictionType;
            TotalBlockedMinutes = totalBlockedMinutes;
        }
    }
}
