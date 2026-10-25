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

        /// <summary>
        /// Створює зміну агрегату для збереження нового обмеження користувача.
        ///
        /// (Creates an aggregate change for persisting a new user restriction.)
        /// </summary>
        /// <param name="restriction">
        /// Створене обмеження користувача.
        ///
        /// (The newly created user restriction.)
        /// </param>
        /// <param name="occurredOn">
        /// Час створення обмеження.
        ///
        /// (The restriction creation time.)
        /// </param>
        public UserRestrictionCreated(
            UserRestriction restriction,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            Restriction = restriction;
        }
    }
}
