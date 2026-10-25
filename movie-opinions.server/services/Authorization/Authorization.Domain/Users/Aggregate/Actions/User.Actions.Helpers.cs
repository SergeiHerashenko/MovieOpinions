using Authorization.Domain.Users.Entities.UsersPendingAction;

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        /// <summary>
        /// Встановлює передану відкладену дію як поточну
        /// дію користувача в агрегаті.
        ///
        /// Використовується під час застосування підготовленого
        /// плану створення або заміни відкладеної дії.
        ///
        /// (Sets the supplied pending action as the user's
        /// current action in the aggregate.
        ///
        /// Used when applying a prepared plan
        /// to create or replace a pending action.)
        /// </summary>
        /// <param name="pendingAction">
        /// Відкладена дія, яку необхідно встановити як поточну.
        ///
        /// (Pending action to set as the current action.)
        /// </param>
        private void SetPendingAction(UserPendingAction pendingAction)
        {
            _action = pendingAction;
        }
    }
}
