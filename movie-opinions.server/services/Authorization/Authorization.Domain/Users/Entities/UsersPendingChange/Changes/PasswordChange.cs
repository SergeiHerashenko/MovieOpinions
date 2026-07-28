using Authorization.Domain.Users.Entities.UsersPendingChange.Enums;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Domain.Users.Entities.UsersPendingChange.Changes
{
    public sealed class PasswordChange : UserChange
    {
        public Password NewPassword { get; }

        internal PasswordChange(Password newPassword)
        {
            NewPassword = newPassword;
        }

        public override string Value => NewPassword.Value;

        public override UserChangeType UserChangeType => UserChangeType.PasswordChange;

        #region Restoration
        public static PasswordChange Restore(PasswordHash hashPassword)
        {
            return new PasswordChange(Password.Restore(hashPassword));
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return NewPassword;
        }
    }
}
