using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;

namespace Authorization.Domain.Users.AggregateChanges.SessionRestriction
{
    /// <summary>
    /// Фіксує створену сесію обмежень як зміну агрегату,
    /// яку потрібно зберегти.
    ///
    /// (Records a newly created restriction session as an aggregate
    /// change to be persisted.)
    /// </summary>
    public sealed class UserRestrictionSessionCreated : AggregateChange
    {
        public UserRestrictionSession UserRestrictionSession { get; }

        /// <summary>
        /// Створює зміну агрегату для збереження нової сесії обмежень.
        ///
        /// (Creates an aggregate change for persisting a new restriction session.)
        /// </summary>
        /// <param name="userRestrictionSession">
        /// Створена сесія обмежень користувача.
        ///
        /// (The newly created user restriction session.)
        /// </param>
        /// <param name="occurredOn">
        /// Час створення сесії.
        ///
        /// (The session creation time.)
        /// </param>
        public UserRestrictionSessionCreated(
            UserRestrictionSession userRestrictionSession,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserRestrictionSession = userRestrictionSession;
        }
    }
}
