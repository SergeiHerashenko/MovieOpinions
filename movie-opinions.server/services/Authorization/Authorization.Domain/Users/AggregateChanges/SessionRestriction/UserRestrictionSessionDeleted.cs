using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;

namespace Authorization.Domain.Users.AggregateChanges.SessionRestriction
{
    /// <summary>
    /// Фіксує необхідність видалити сесію обмежень користувача зі сховища.
    ///
    /// (Records the need to delete a user restriction session from persistence.)
    /// </summary>
    public sealed class UserRestrictionSessionDeleted : AggregateChange
    {
        public UserRestrictionSessionId UserRestrictionSessionId { get; }

        /// <summary>
        /// Створює зміну агрегату для видалення сесії обмежень зі сховища.
        ///
        /// (Creates an aggregate change for deleting a restriction session
        /// from persistence.)
        /// </summary>
        /// <param name="userRestrictionSessionId">
        /// Ідентифікатор сесії, яку потрібно видалити.
        ///
        /// (Identifier of the session to delete.)
        /// </param>
        /// <param name="occurredOn">
        /// Час фіксації видалення сесії.
        ///
        /// (The time when session deletion was recorded.)
        /// </param>
        public UserRestrictionSessionDeleted(
            UserRestrictionSessionId userRestrictionSessionId,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSessionId = userRestrictionSessionId;
        }
    }
}
