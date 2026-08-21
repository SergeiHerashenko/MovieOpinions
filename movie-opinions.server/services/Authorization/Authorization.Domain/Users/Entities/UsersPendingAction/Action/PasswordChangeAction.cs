using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.Action
{
    public sealed class PasswordChangeAction : UserAction
    {
        public Password NewPassword { get; }

        internal PasswordChangeAction(Password newPassword)
        {
            NewPassword = newPassword;
        }

        public string Value => NewPassword.Value;

        public override UserActionType ActionType => UserActionType.ActionChangePassword;

        #region Restoration
        public static PasswordChangeAction Restore(PasswordHash hashPassword)
        {
            return new PasswordChangeAction(Password.Restore(hashPassword));
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return NewPassword;
            yield return ActionType;
        }
    }
}
