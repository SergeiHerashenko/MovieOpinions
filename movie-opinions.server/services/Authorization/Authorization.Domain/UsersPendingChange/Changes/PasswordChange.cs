using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersPendingChange.Enums;

namespace Authorization.Domain.UsersPendingChange.Changes
{
    public sealed class PasswordChange : UserChange
    {
        public Password NewPassword { get; }

        public PasswordChange(Password newPassword)
        {
            NewPassword = newPassword;
        }

        public override string Value => NewPassword.Value;

        public override UserChangeType Type => UserChangeType.PasswordChange;

        public static PasswordChange Restore(string hashPassword)
        {
            return new PasswordChange(Password.Restore(hashPassword));
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return NewPassword;
        }
    }
}
