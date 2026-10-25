using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.ValueObjects.EmailUser.Validation.Emails;

namespace Authorization.Domain.Common.ValueObjects.EmailUser
{
    /// <summary>
    /// Представляє нормалізовану email-адресу, сформовану
    /// з локальної та доменної частин.
    ///
    /// (Represents a normalized email address composed
    /// of local and domain parts.)
    /// </summary>
    public sealed class Email : ValueObject
    {
        /// <summary>
        /// Локальна частина email-адреси, розташована перед символом '@'.
        ///
        /// (Local part of the email address located before the '@' character.)
        /// </summary>
        public EmailLocalPart EmailLocalPart { get; }

        /// <summary>
        /// Доменна частина email-адреси, розташована після символу '@'.
        ///
        /// (Domain part of the email address located after the '@' character.)
        /// </summary>
        public EmailDomainPart EmailDomainPart { get; }

        private Email(
            EmailLocalPart emailLocalPart,
            EmailDomainPart emailDomainPart)
        {
            EmailLocalPart = emailLocalPart;
            EmailDomainPart = emailDomainPart;
        }

        #region Creation
        /// <summary>
        /// Нормалізує зовнішнє значення, перевіряє загальну структуру
        /// email-адреси та створює її локальну й доменну частини.
        ///
        /// Детальні правила локальної та доменної частин перевіряються
        /// відповідними value objects.
        ///
        /// (Normalizes an external value, validates the overall email
        /// structure, and creates its local and domain parts.
        ///
        /// Detailed local-part and domain-part rules are validated
        /// by the corresponding value objects.)
        /// </summary>
        /// <param name="rawEmail">
        /// Зовнішнє значення email-адреси. Може бути null,
        /// що розглядається як очікувана помилка валідації.
        ///
        /// (The external email-address value. null is treated
        /// as an expected validation failure.)
        /// </param>
        /// <returns>
        /// Успішний результат із нормалізованою email-адресою
        /// або перша виявлена помилка валідації.
        ///
        /// (A successful result containing the normalized email
        /// address, or the first validation error.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        public static Result<Email> Create(string? rawEmail)
        {
            var normalizedEmail = rawEmail?.Trim() ?? string.Empty;

            var failure = EmailValidator.ValidateForError(
                OperationType.Create,
                normalizedEmail
            );

            if (failure is not null)
                return Result<Email>.Failure(failure.Value);

            var (localPart, domainPart) = Split(normalizedEmail);

            var localResult = EmailLocalPart.Create(localPart);

            if (localResult.IsFailure)
                return Result<Email>.Failure(localResult.Errors);

            var domainResult = EmailDomainPart.Create(domainPart);

            if (domainResult.IsFailure)
                return Result<Email>.Failure(domainResult.Errors);

            return Result<Email>.Success(new Email(localResult.Value, domainResult.Value));
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює email-адресу зі збереженого канонічного значення.
        ///
        /// Загальна структура перевіряється валідатором email-адреси,
        /// а структурна коректність і канонічність складових —
        /// відповідними value objects.
        ///
        /// (Restores an email address from its persisted canonical value.
        ///
        /// The overall structure is validated by the email validator,
        /// while the validity and canonical form of its components
        /// are validated by the corresponding value objects.)
        /// </summary>
        /// <param name="storedEmail">
        /// Збережене значення email-адреси.
        ///
        /// (The persisted email-address value.)
        /// </param>
        /// <returns>
        /// Відновлена email-адреса.
        ///
        /// (The restored email address.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережена email-адреса або одна з її
        /// складових порушує доменні правила.
        ///
        /// (Thrown when the persisted email address or one
        /// of its components violates domain rules.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        public static Email Restore(string storedEmail)
        {
            var failure = EmailValidator.ValidateForException(
                OperationType.Restore,
                storedEmail
            );

            if (failure is not null)
                throw failure.Value;

            var (localPart, domainPart) = Split(storedEmail);

            var emailLocalPart = EmailLocalPart.Restore(localPart);

            var emailDomainPart = EmailDomainPart.Restore(domainPart);

            return new Email(emailLocalPart, emailDomainPart);
        }
        #endregion

        /// <summary>
        /// Розділяє попередньо перевірену email-адресу
        /// на локальну та доменну частини.
        ///
        /// Метод повинен викликатися лише після успішної перевірки,
        /// що значення містить рівно один символ '@' і дві непорожні частини.
        ///
        /// (Splits a previously validated email address
        /// into its local and domain parts.
        ///
        /// The method must be called only after successful validation
        /// that the value contains exactly one '@' character
        /// and two non-empty parts.)
        /// </summary>
        /// <param name="email">
        /// Попередньо перевірена email-адреса.
        ///
        /// (The previously validated email address.)
        /// </param>
        /// <returns>
        /// Локальна та доменна частини email-адреси.
        ///
        /// (The local and domain parts of the email address.)
        /// </returns>
        private static (string LocalPart, string DomainPart) Split(string email)
        {
            var separatorIndex = email.IndexOf('@');

            return (
                email[..separatorIndex],
                email[(separatorIndex + 1)..]
            );
        }

        /// <summary>
        /// Повертає повне канонічне представлення email-адреси.
        ///
        /// (Returns the complete canonical representation
        /// of the email address.)
        /// </summary>
        /// <returns>
        /// Повна email-адреса у канонічному форматі.
        ///
        /// (The complete email address in canonical form.)
        /// </returns>
        public string GetFullEmail()
        {
            return $"{EmailLocalPart.Value}@{EmailDomainPart.Value}";
        }

        /// <inheritdoc/>
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return EmailLocalPart;
            yield return EmailDomainPart;
        }
    }
}
