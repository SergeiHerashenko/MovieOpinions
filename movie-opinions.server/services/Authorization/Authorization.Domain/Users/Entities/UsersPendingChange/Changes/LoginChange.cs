using Authorization.Domain.Users.Entities.UsersPendingChange.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;

namespace Authorization.Domain.Users.Entities.UsersPendingChange.Changes
{
    public sealed class LoginChange : UserChange
    {
        public Login NewLogin { get; }

        internal LoginChange(Login newLogin)
        {
            NewLogin = newLogin;
        }

        public override string Value => NewLogin.Value;

        public override UserChangeType UserChangeType => UserChangeType.LoginChange;

        #region Restoration
        public static LoginChange Restore(Email email)
        {
            return new LoginChange(Login.From(email));
        }

        public static LoginChange Restore(Phone phone)
        {
            return new LoginChange(Login.From(phone));
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return NewLogin;
        }
    }
}
