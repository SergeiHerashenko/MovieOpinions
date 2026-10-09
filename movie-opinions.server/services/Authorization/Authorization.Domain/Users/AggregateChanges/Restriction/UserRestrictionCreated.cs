using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestriction;

namespace Authorization.Domain.Users.AggregateChanges.Restriction
{
    /// <summary>
    /// Фіксує новостворене обмеження для збереження.
    ///
    /// (Records a newly created restriction for persistence.)
    /// </summary>
    public sealed class UserRestrictionCreated : AggregateChange
    {
        public UserRestriction Restriction { get; }

        public UserRestrictionCreated(
            UserRestriction restriction,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            Restriction = restriction;
        }
    }
}
