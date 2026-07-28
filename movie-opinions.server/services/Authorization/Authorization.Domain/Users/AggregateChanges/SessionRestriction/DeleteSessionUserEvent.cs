using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;

namespace Authorization.Domain.Users.AggregateChanges.SessionRestriction
{
    public sealed class DeleteSessionUserEvent : AggregateChange
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        public DeleteSessionUserEvent(
            UserRestrictionSessionId userRestrictionSessionId,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSessionId = userRestrictionSessionId;
        }
    }
}
