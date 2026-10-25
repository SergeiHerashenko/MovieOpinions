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

        /// <summary>
        /// Створює зміну агрегату для збереження оновленого обмеження.
        ///
        /// (Creates an aggregate change for persisting an updated restriction.)
        /// </summary>
        /// <param name="userRestriction">
        /// Обмеження користувача з оновленим станом.
        ///
        /// (The user restriction with its updated state.)
        /// </param>
        /// <param name="occurredOn">
        /// Час зміни стану обмеження.
        ///
        /// (The time when the restriction state changed.)
        /// </param>
        public UserRestrictionUpdated(
            UserRestriction userRestriction,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestriction = userRestriction;
        }
    }
}
