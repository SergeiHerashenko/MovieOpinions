using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.Actions
{
    /// <summary>
    /// Представляє дані відкладеної дії зі зміни логіна користувача.
    ///
    /// (Represents the data of a pending user login-change action.)
    /// </summary>
    public sealed class ChangeLoginAction : UserAction
    {
        /// <summary>
        /// Новий логін, який буде встановлено після підтвердження дії.
        ///
        /// (New login that will be applied after action confirmation.)
        /// </summary>
        public Login NewLogin { get; }

        /// <summary>
        /// Тип нового логіна.
        ///
        /// (Type of the new login.)
        /// </summary>
        public LoginType LoginType => NewLogin.Type;

        private ChangeLoginAction(Login newLogin)
        {
            NewLogin = newLogin;
        }

        public override string Value => NewLogin.Value;

        public override UserActionType ActionType => UserActionType.ChangeLogin;

        #region Creation
        /// <summary>
        /// Створює дію зміни логіна з валідного доменного логіна.
        ///
        /// (Creates a login-change action from a valid domain login.)
        /// </summary>
        internal static ChangeLoginAction Create(Login login)
        {
            DomainGuard.AgainstNull<ChangeLoginAction>(
                OperationType.Create,
                (login, nameof(login))
            );

            return new ChangeLoginAction(login);
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює дію зміни логіна з відновленого доменного логіна.
        ///
        /// (Restores a login-change action from a restored domain login.)
        /// </summary>
        public static ChangeLoginAction Restore(Email email)
        {
            DomainGuard.AgainstNull<ChangeLoginAction>(
                OperationType.Restore,
                (email, nameof(email))
            );

            return new ChangeLoginAction(Login.From(email));
        }

        public static ChangeLoginAction Restore(Phone phone)
        {
            DomainGuard.AgainstNull<ChangeLoginAction>(
                OperationType.Restore,
                (phone, nameof(phone))
            );

            return new ChangeLoginAction(Login.From(phone));
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return NewLogin;
        }
    }
}
