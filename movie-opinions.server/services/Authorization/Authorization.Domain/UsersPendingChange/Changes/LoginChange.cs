using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingChange.Enums;

namespace Authorization.Domain.UsersPendingChange.Changes
{
    public sealed class LoginChange : UserChange
    {
        public Login NewLogin { get; }

        public LoginChange(Login newLogin)
        {
            NewLogin = newLogin;
        }

        public override string Value => NewLogin.Value;

        public override UserChangeType Type => UserChangeType.LoginChange;

        public static LoginChange Restore(string newLogin, LoginType type, string? countryCode = null)
        {
            return new LoginChange(Login.Restore(newLogin, type, countryCode));
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return NewLogin;
        }
    }
}
