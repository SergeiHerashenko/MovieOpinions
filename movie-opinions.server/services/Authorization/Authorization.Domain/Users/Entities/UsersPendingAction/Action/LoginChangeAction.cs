using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.Action
{
    public sealed class LoginChangeAction : UserAction
    {
        public Login NewLogin { get; }

        public LoginType LoginType => NewLogin.Type;

        internal LoginChangeAction(Login newLogin)
        {
            NewLogin = newLogin;
        }

        public string Value => NewLogin.Value;

        public override UserActionType ActionType => UserActionType.ActionChangeLogin;

        #region Restoration
        public static LoginChangeAction Restore(Email email)
        {
            return new LoginChangeAction(Login.From(email));
        }

        public static LoginChangeAction Restore(Phone phone)
        {
            return new LoginChangeAction(Login.From(phone));
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return NewLogin;
            yield return ActionType;
        }
    }
}
