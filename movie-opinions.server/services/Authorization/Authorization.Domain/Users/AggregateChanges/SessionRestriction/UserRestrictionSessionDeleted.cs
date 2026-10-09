using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;

namespace Authorization.Domain.Users.AggregateChanges.SessionRestriction
{
    /// <summary>
    /// Фіксує необхідність видалити порожню сесію обмежень зі сховища.
    ///
    /// (Records the removal of an empty restriction session
    /// from persistence.)
    /// </summary>
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
