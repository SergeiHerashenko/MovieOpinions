using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.Action
{
    public sealed class DeleteAccountAction : UserAction
    {
        public DeletionReason Reason { get; }

        internal DeleteAccountAction(DeletionReason reason)
        {
            Reason = reason;
        }

        public override UserActionType ActionType => UserActionType.ActionDeleteUser;

        #region Restoration
        public static DeleteAccountAction Restore(DeletionReason reason)
        {
            return new DeleteAccountAction(reason);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Reason;
            yield return ActionType;
        }
    }
}
