using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.Actions
{
    /// <summary>
    /// Представляє дані відкладеної дії з видалення користувача.
    ///
    /// (Represents the data of a pending user-deletion action.)
    /// </summary>
    public sealed class DeleteUserAction : UserAction
    {
        /// <summary>
        /// Причина видалення, яка буде використана
        /// після підтвердження дії.
        ///
        /// (Deletion reason that will be used
        /// after the action is confirmed.)
        /// </summary>
        public DeletionReason Reason { get; }

        private DeleteUserAction(DeletionReason reason)
        {
            Reason = reason;
        }

        public override string Value => Reason.Value;

        public override UserActionType ActionType => UserActionType.DeleteUser;

        #region Creation
        /// <summary>
        /// Створює дані відкладеної дії з видалення користувача
        /// з попередньо перевіреної причини видалення.
        ///
        /// (Creates pending user-deletion action data
        /// from a previously validated deletion reason.)
        /// </summary>
        /// <param name="reason">
        /// Доменне представлення причини видалення.
        ///
        /// (The domain representation of the deletion reason.)
        /// </param>
        /// <returns>
        /// Створена дія видалення користувача.
        ///
        /// (The created user-deletion action.)
        /// </returns>
        internal static DeleteUserAction Create(DeletionReason reason)
        {
            DomainGuard.AgainstNull<DeleteUserAction>(
                OperationType.Create,
                (reason, nameof(reason))
            );

            return new DeleteUserAction(reason);
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює дані відкладеної дії з видалення користувача
        /// з попередньо відновленої причини видалення.
        ///
        /// (Restores pending user-deletion action data
        /// from a previously restored deletion reason.)
        /// </summary>
        /// <param name="reason">
        /// Відновлена причина видалення.
        ///
        /// (The restored deletion reason.)
        /// </param>
        /// <returns>
        /// Відновлена дія видалення користувача.
        ///
        /// (The restored user-deletion action.)
        /// </returns>
        public static DeleteUserAction Restore(DeletionReason reason)
        {
            DomainGuard.AgainstNull<DeleteUserAction>(
                OperationType.Restore,
                (reason, nameof(reason))
            );

            return new DeleteUserAction(reason);
        }
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Reason;
        }
    }
}
