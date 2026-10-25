using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingAction;

namespace Authorization.Domain.Users.AggregateChanges.Action
{
    /// <summary>
    /// Фіксує новостворену відкладену дію користувача
    /// як зміну агрегату, яку потрібно зберегти.
    ///
    /// (Records a newly created pending user action
    /// as an aggregate change to be persisted.)
    /// </summary>
    public class UserPendingActionCreated : AggregateChange
    {
        public UserPendingAction UserPendingAction { get; }

        public DateTimeOffset Now { get; }

        /// <summary>
        /// Створює зміну агрегату для збереження
        /// нової відкладеної дії користувача.
        ///
        /// (Creates an aggregate change for persisting
        /// a new pending user action.)
        /// </summary>
        /// <param name="userPendingAction">
        /// Створена відкладена дія користувача.
        /// (Created pending user action.)
        /// </param>
        /// <param name="now">
        /// Час створення відкладеної дії.
        /// (Time the pending action was created.)
        /// </param>
        public UserPendingActionCreated(
            UserPendingAction userPendingAction,
            DateTimeOffset now)
            : base(now)
        {
            UserPendingAction = userPendingAction;
            Now = now;
        }
    }
}
