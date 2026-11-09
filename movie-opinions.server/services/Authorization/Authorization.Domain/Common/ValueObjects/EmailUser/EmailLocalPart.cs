using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.EmailUser.Validation.LocalPart;
using Authorization.Domain.Results;

namespace Authorization.Domain.Common.ValueObjects.EmailUser
{
    /// <summary>
    /// Представляє нормалізовану локальну частину email-адреси,
    /// розташовану перед символом '@'.
    ///
    /// Значення зберігається у визначеному системою канонічному
    /// форматі: у нижньому регістрі та без зовнішніх пробілів.
    ///
    /// (Represents the normalized local part of an email address
    /// located before the '@' character.
    ///
    /// The value is stored in the system-defined canonical format:
    /// lowercase and without surrounding whitespace.)
    /// </summary>
    public sealed class EmailLocalPart : ValueObject
    {
        /// <summary>
        /// Канонічне значення локальної частини email-адреси.
        ///
        /// (Canonical value of the email local part.)
        /// </summary>
        public string Value { get; }

        private EmailLocalPart(string value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Нормалізує зовнішнє значення та створює локальну
        /// частину email-адреси після успішної валідації.
        ///
        /// (Normalizes an external value and creates an email local part
        /// after successful validation.)
        /// </summary>
        /// <param name="rawEmailLocalPart">
        /// Зовнішнє значення локальної частини email-адреси.
        /// null перетворюється на порожній рядок перед валідацією.
        ///
        /// (The external email local-part value.
        /// null is converted to an empty string before validation.)
        /// </param>
        /// <returns>
        /// Успішний результат із локальною частиною або перша
        /// виявлена помилка валідації.
        ///
        /// (A successful result containing the email local
        /// part, or the first validation error.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        internal static Result<EmailLocalPart> Create(string rawEmailLocalPart)
        {
            var normalizedLocalPart = rawEmailLocalPart?
                .Trim()
                .ToLowerInvariant()
                ?? string.Empty;

            var failure = EmailLocalValidator.ValidateForError(
                OperationType.Create,
                normalizedLocalPart
            );

            if (failure is not null)
            {
                return Result<EmailLocalPart>.Failure(failure.Value);
            }

            return Result<EmailLocalPart>.Success(new EmailLocalPart(normalizedLocalPart));
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює локальну частину email-адреси зі збереженого
        /// канонічного значення.
        ///
        /// Окрім звичайної валідації, перевіряє, що значення вже
        /// збережене у нижньому регістрі та не містить зовнішніх пробілів.
        ///
        /// (Restores the email local part from its persisted canonical value.
        ///
        /// In addition to regular validation, verifies that the value
        /// is already lowercase and contains no surrounding whitespace.)
        /// </summary>
        /// <param name="storedEmailLocalPart">
        /// Збережене значення локальної частини email-адреси.
        ///
        /// (The persisted email local-part value.)
        /// </param>
        /// <returns>
        /// Відновлена локальна частина email-адреси.
        ///
        /// (The restored email local part.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення порушує правила
        /// локальної частини або не відповідає канонічному формату.
        ///
        /// (Thrown when the persisted value violates local-part
        /// rules or does not match the canonical format.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        public static EmailLocalPart Restore(string storedEmailLocalPart)
        {
            var failure = EmailLocalValidator.ValidateForException(
                OperationType.Restore,
                storedEmailLocalPart
            );

            if (failure is not null)
            {
                throw failure.Value;
            }

            var normalizedLocalPart = storedEmailLocalPart.Trim().ToLowerInvariant();

            if (!string.Equals(
                storedEmailLocalPart,
                normalizedLocalPart,
                StringComparison.Ordinal))
            {
                throw DomainDataInconsistencyException.InvalidFieldFormat<EmailLocalPart>(
                    nameof(Value),
                    storedEmailLocalPart,
                    OperationType.Restore
                );
            }

            return new EmailLocalPart(storedEmailLocalPart);
        }
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
