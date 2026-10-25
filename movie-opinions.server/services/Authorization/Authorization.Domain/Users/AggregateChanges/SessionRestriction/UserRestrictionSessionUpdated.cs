using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;

namespace Authorization.Domain.Users.AggregateChanges.SessionRestriction
{
    /// <summary>
    /// Фіксує оновлену сесію обмежень як зміну агрегату,
    /// яку потрібно зберегти.
    ///
    /// (Records an updated restriction session as an aggregate
    /// change to be persisted.)
    /// </summary>
    public sealed class UserRestrictionSessionUpdated : AggregateChange
    {
        public UserRestrictionSession UserRestrictionSession { get; }

        /// <summary>
        /// Створює зміну агрегату для збереження оновленої сесії обмежень.
        ///
        /// (Creates an aggregate change for persisting an updated restriction session.)
        /// </summary>
        /// <param name="userRestrictionSession">
        /// Оновлена сесія обмежень користувача.
        ///
        /// (The updated user restriction session.)
        /// </param>
        /// <param name="occurredOn">
        /// Час оновлення сесії.
        ///
        /// (The session update time.)
        /// </param>
        public UserRestrictionSessionUpdated(
            UserRestrictionSession userRestrictionSession,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSession = userRestrictionSession;
        }
    }
}
