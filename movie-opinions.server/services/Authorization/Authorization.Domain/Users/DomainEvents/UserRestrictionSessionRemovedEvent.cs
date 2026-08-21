using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserRestrictionSessionRemovedEvent : DomainEvent
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        public Login Login { get; }

        public RestrictionType RestrictionType { get; }

        public DateTimeOffset RestrictionCanselDate { get; }

        public UserRestrictionSessionRemovedEvent(
            UserRestrictionSessionId userRestrictionSessionId, 
            Login login,
            RestrictionType restrictionType, 
            DateTimeOffset restrictionCanselDate)
            : base(restrictionCanselDate)
        {
            UserRestrictionSessionId = userRestrictionSessionId;
            Login = login;
            RestrictionType = restrictionType;
            RestrictionCanselDate = restrictionCanselDate;
        }
    }
}
