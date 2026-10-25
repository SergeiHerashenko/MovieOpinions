using Authorization.Domain.Common.Enums;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PhoneUser;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

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
        /// Створює дані відкладеної дії зі зміни логіна
        /// з попередньо перевіреного доменного логіна.
        ///
        /// (Creates pending login-change action data
        /// from a previously validated domain login.)
        /// </summary>
        /// <param name="login">
        /// Новий доменний логін користувача.
        ///
        /// (The new domain user login.)
        /// </param>
        /// <returns>
        /// Створена дія зміни логіна.
        ///
        /// (The created login-change action.)
        /// </returns>
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
        /// Відновлює дані відкладеної дії зі зміни логіна
        /// з попередньо відновленої email-адреси.
        ///
        /// (Restores pending login-change action data
        /// from a previously restored email address.)
        /// </summary>
        /// <param name="email">
        /// Відновлена email-адреса нового логіна.
        ///
        /// (The restored email address of the new login.)
        /// </param>
        /// <returns>
        /// Відновлена дія зміни логіна на основі email-адреси.
        ///
        /// (The restored login-change action based on an email address.)
        /// </returns>
        public static ChangeLoginAction Restore(Email email)
        {
            DomainGuard.AgainstNull<ChangeLoginAction>(
                OperationType.Restore,
                (email, nameof(email))
            );

            return new ChangeLoginAction(Login.From(email));
        }

        /// <summary>
        /// Відновлює дані відкладеної дії зі зміни логіна
        /// з попередньо відновленого телефонного номера.
        ///
        /// (Restores pending login-change action data
        /// from a previously restored telephone number.)
        /// </summary>
        /// <param name="phone">
        /// Відновлений телефонний номер нового логіна.
        ///
        /// (The restored telephone number of the new login.)
        /// </param>
        /// <returns>
        /// Відновлена дія зміни логіна на основі телефонного номера.
        ///
        /// (The restored login-change action based on a telephone number.)
        /// </returns>
        public static ChangeLoginAction Restore(Phone phone)
        {
            DomainGuard.AgainstNull<ChangeLoginAction>(
                OperationType.Restore,
                (phone, nameof(phone))
            );

            return new ChangeLoginAction(Login.From(phone));
        }
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return NewLogin;
        }
    }
}
