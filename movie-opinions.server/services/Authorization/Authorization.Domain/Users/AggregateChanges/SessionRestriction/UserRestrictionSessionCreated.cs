using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;

namespace Authorization.Domain.Users.AggregateChanges.SessionRestriction
{
    public sealed class UserRestrictionSessionCreated : AggregateChange
    {
        public UserRestrictionSession UserRestrictionSession { get; }

        public UserRestrictionSessionCreated(
            UserRestrictionSession userRestrictionSession,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSession = userRestrictionSession;
        }
    }
}
