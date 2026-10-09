using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction;

namespace Authorization.Domain.Users.AggregateChanges.Restriction
{
    /// <summary>
    /// Фіксує змінений стан обмеження для збереження.
    ///
    /// (Records the changed state of a restriction for persistence.)
    /// </summary>
    public sealed class UserRestrictionUpdated : AggregateChange
    {
        public UserRestriction UserRestriction { get; }

        public UserRestrictionUpdated(
            UserRestriction userRestriction,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestriction = userRestriction;
        }
    }
}
