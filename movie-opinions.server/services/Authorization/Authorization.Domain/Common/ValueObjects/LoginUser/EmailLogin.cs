using Authorization.Domain.Common.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.Common.ValueObjects.LoginUser
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

        /// <summary>
        /// Ініціалізує email-логін із перевіреної email-адреси.
        ///
        /// (Initializes an email login from a validated email address.)
        /// </summary>
        /// <param name="email">
        /// Перевірена email-адреса.
        ///
        /// (The validated email address.)
        /// </param>
        internal EmailLogin(Email email)
        {
            Email = email;
        }

        /// <inheritdoc/>
        public override string Value => Email.GetFullEmail();

        /// <inheritdoc/>
        public override LoginType Type => LoginType.Email;

        #region Restoration
        /// <summary>
        /// Відновлює email-логін зі збереженого канонічного значення.
        ///
        /// (Restores an email login from its persisted canonical value.)
        /// </summary>
        /// <param name="storedEmail">
        /// Збережене значення email-адреси.
        ///
        /// (The persisted email-address value.)
        /// </param>
        /// <returns>
        /// Відновлений email-логін.
        ///
        /// (The restored email login.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережена email-адреса порушує доменні правила
        /// або не відповідає канонічному формату.
        ///
        /// (Thrown when the persisted email address violates domain
        /// rules or does not match the canonical format.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// правил валідації email-адреси.
        ///
        /// (Thrown when an internal precondition of the
        /// email-address validation rules is violated.)
        /// </exception>
        public static EmailLogin Restore(string storedEmail)
        {
            var email = Email.Restore(storedEmail);

            return new EmailLogin(email);
        }
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Email;
        }
    }
}
