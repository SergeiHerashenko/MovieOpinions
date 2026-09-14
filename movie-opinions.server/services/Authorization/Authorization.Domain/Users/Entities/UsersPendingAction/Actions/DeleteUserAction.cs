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
        /// Створює дію видалення користувача
        /// з валідної причини видалення.
        ///
        /// (Creates a user-deletion action
        /// from a valid deletion reason.)
        /// </summary>
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
        /// Відновлює дію видалення користувача
        /// з відновленої причини видалення.
        ///
        /// (Restores a user-deletion action
        /// from a restored deletion reason.)
        /// </summary>
        public static DeleteUserAction Restore(DeletionReason reason)
        {
            DomainGuard.AgainstNull<DeleteUserAction>(
                OperationType.Restore,
                (reason, nameof(reason))
            );

            return new DeleteUserAction(reason);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Reason;
        }
    }
}
