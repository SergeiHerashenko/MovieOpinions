using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersPendingChange.Enums;

namespace Authorization.Domain.UsersPendingChange.Changes
{
    public sealed class PasswordReset : UserChange
    {
        public Password NewPassword { get; }

        public PasswordReset(Password newPassword)
        {
            NewPassword = newPassword;
        }

        public override string Value => NewPassword.HashPassword;

        public override UserChangeType Type => UserChangeType.PasswordReset;

        public static PasswordReset Restore(string hashPassword)
        {
            return new PasswordReset(Password.Restore(hashPassword));
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return NewPassword;
        }
    }
}
