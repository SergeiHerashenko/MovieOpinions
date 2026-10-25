using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingAction;

namespace Authorization.Domain.Users.AggregateChanges.Action
{
    /// <summary>
    /// Фіксує оновлену відкладену дію користувача
    /// як зміну агрегату, яку потрібно зберегти.
    ///
    /// (Records an updated pending user action
    /// as an aggregate change to be persisted.)
    /// </summary>
    public class UserPendingActionUpdated : AggregateChange
    {
        public UserPendingAction UserPendingAction { get; }

        public DateTimeOffset Now { get; }

        /// <summary>
        /// Створює зміну агрегату для збереження
        /// оновленої відкладеної дії користувача.
        ///
        /// (Creates an aggregate change for persisting
        /// an updated pending user action.)
        /// </summary>
        /// <param name="userPendingAction">
        /// Оновлена відкладена дія користувача.
        /// (Updated pending user action.)
        /// </param>
        /// <param name="now">
        /// Час оновлення відкладеної дії.
        /// (Time the pending action was updated.)
        /// </param>
        public UserPendingActionUpdated(
            UserPendingAction userPendingAction,
            DateTimeOffset now)
            : base(now)
        {
            UserPendingAction = userPendingAction;
            Now = now;
        }
    }
}
