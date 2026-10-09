using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser;

namespace Authorization.Domain.Users.ValueObjects.LoginUser
{
    /// <summary>
    /// Представляє логін користувача на основі email-адреси.
    ///
    /// (Represents a user login based on an email address.)
    /// </summary>
    public sealed class EmailLogin : Login
    {
        /// <summary>
        /// Email-адреса, що використовується як логін користувача.
        ///
        /// (Email address used as the user login.)
        /// </summary>
        public Email Email { get; }

        internal EmailLogin(Email email)
        {
            Email = email;
        }

        public override string Value => Email.GetFullEmail();

        public override LoginType Type => LoginType.Email;

        #region Restoration
        /// <summary>
        /// Відновлює email-логін зі збереженого канонічного значення.
        ///
        /// (Restores an email login from its persisted canonical value.)
        /// </summary>
        /// <param name="storedEmail">
        /// Збережене значення email-адреси.
        /// </param>
        /// <returns>Відновлений email-логін.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережена email-адреса порушує доменні правила
        /// або не відповідає канонічному формату.
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// правил валідації email-адреси.
        /// </exception>
        public static EmailLogin Restore(string storedEmail)
        {
            var email = Email.Restore(storedEmail);

            return new EmailLogin(email);
        }
        #endregion

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Email;
        }
    }
}
