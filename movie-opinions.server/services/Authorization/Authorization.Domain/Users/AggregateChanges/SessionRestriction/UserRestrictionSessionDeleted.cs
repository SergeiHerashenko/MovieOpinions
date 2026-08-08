using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;

namespace Authorization.Domain.Users.AggregateChanges.SessionRestriction
{
    public sealed class UserRestrictionSessionDeleted : AggregateChange
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        public UserRestrictionSessionDeleted(
            UserRestrictionSessionId userRestrictionSessionId,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSessionId = userRestrictionSessionId;
        }
    }
}
