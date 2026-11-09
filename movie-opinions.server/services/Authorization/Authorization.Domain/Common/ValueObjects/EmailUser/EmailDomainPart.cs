using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.EmailUser.Validation.DomainPart;
using Authorization.Domain.Results;

namespace Authorization.Domain.Common.ValueObjects.EmailUser
{
    /// <summary>
    /// Представляє нормалізовану доменну частину email-адреси,
    /// розташовану після символу '@'.
    ///
    /// (Represents the normalized domain part of an email address
    /// located after the '@' character.)
    /// </summary>
    public sealed class EmailDomainPart : ValueObject
    {
        /// <summary>
        /// Канонічне значення домену в нижньому регістрі
        /// без зовнішніх пробілів.
        ///
        /// (Canonical lowercase domain value without surrounding whitespace.)
        /// </summary>
        public string Value { get; }

        private EmailDomainPart(string value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Нормалізує зовнішнє значення, виконує структурну валідацію
        /// та перевіряє політики, застосовні до нових email-адрес.
        ///
        /// (Normalizes an external value, performs structural validation,
        /// and validates policies applicable to new email addresses.)
        /// </summary>
        /// <param name="rawEmailDomain">
        /// Зовнішнє значення доменної частини email-адреси.
        ///
        /// (The external email domain-part value.)
        /// </param>
        /// <returns>
        /// Успішний результат із доменною частиною або перша
        /// виявлена помилка валідації.
        ///
        /// (A successful result containing the email domain
        /// part, or the first validation error.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        internal static Result<EmailDomainPart> Create(string rawEmailDomain)
        {
            var normalizedDomain = rawEmailDomain?.Trim().ToLowerInvariant()
                ?? string.Empty;

            var failure = EmailDomainValidator.ValidateForError(
                OperationType.Create,
                normalizedDomain
            );

            if (failure is not null)
            {
                return Result<EmailDomainPart>.Failure(failure.Value);
            }

            return Result<EmailDomainPart>.Success(new EmailDomainPart(normalizedDomain));
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює доменну частину email-адреси зі збереженого
        /// канонічного значення.
        ///
        /// Під час відновлення перевіряються лише структурні правила.
        /// Актуальні політики, застосовні до створення нових email-адрес,
        /// до збереженого значення не застосовуються.
        ///
        /// (Restores the email domain part from its persisted canonical value.
        ///
        /// Only structural rules are validated during restoration.
        /// Current policies applicable to new email addresses are not applied
        /// to an already persisted value.)
        /// </summary>
        /// <param name="storedEmailDomain">
        /// Збережене значення доменної частини email-адреси.
        ///
        /// (The persisted email domain-part value.)
        /// </param>
        /// <returns>
        /// Відновлена доменна частина email-адреси.
        ///
        /// (The restored email domain part.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення структурно некоректне
        /// або не відповідає канонічному формату.
        ///
        /// (Thrown when the persisted value is structurally invalid
        /// or does not match the canonical format.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        public static EmailDomainPart Restore(string storedEmailDomain)
        {
            var failure = EmailDomainValidator.ValidateForException(
                OperationType.Restore,
                storedEmailDomain
            );

            if (failure is not null)
            {
                throw failure.Value;
            }

            var normalizedDomain = storedEmailDomain.Trim().ToLowerInvariant();

            if (!string.Equals(
                storedEmailDomain,
                normalizedDomain,
                StringComparison.Ordinal))
            {
                throw DomainDataInconsistencyException.InvalidFieldFormat<EmailDomainPart>(
                    nameof(Value),
                    storedEmailDomain,
                    OperationType.Restore
                );
            }

            return new EmailDomainPart(storedEmailDomain);
        }
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
