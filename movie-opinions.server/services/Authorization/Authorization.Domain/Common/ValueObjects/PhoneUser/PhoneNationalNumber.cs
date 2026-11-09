using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.NationalNumber;
using Authorization.Domain.Results;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser
{
    /// <summary>
    /// Представляє національну частину телефонного номера
    /// без міжнародного коду країни.
    ///
    /// Значення зберігається у канонічному форматі, що містить
    /// лише ASCII-цифри. Під час створення дозволене зовнішнє
    /// форматування пробільними символами, дефісами та дужками.
    ///
    /// (Represents the national part of a telephone number
    /// without the international country code.
    ///
    /// The value is stored in a canonical format containing only
    /// ASCII digits. External formatting with whitespace, hyphens,
    /// and parentheses is accepted during creation.)
    /// </summary>
    public sealed class PhoneNationalNumber : ValueObject
    {
        /// <summary>
        /// Канонічне значення національного номера,
        /// що містить лише ASCII-цифри.
        ///
        /// (Canonical national-number value containing only ASCII digits.)
        /// </summary>
        public string Value { get; }

        private PhoneNationalNumber(string value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Перевіряє зовнішнє представлення національного номера,
        /// видаляє дозволені символи форматування та створює
        /// його канонічне доменне представлення.
        ///
        /// (Validates an external national-number representation,
        /// removes permitted formatting characters, and creates
        /// its canonical domain representation.)
        /// </summary>
        /// <param name="rawNationalNumber">
        /// Зовнішнє представлення національного номера.
        /// Значення не повинно бути null.
        ///
        /// (The external national-number representation.
        /// The value must not be null.)
        /// </param>
        /// <returns>
        /// Успішний результат із канонічним номером або перша
        /// виявлена помилка валідації.
        ///
        /// (A successful result containing the canonical national
        /// number, or the first validation error.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        public static Result<PhoneNationalNumber> Create(string rawNationalNumber)
        {
            var failure = NationalNumberValidator.ValidateForError(
                OperationType.Create,
                rawNationalNumber
            );

            if (failure is not null)
            {
                return Result<PhoneNationalNumber>.Failure(failure.Value);
            }

            var normalizedNationalNumber = Normalize(rawNationalNumber);

            return Result<PhoneNationalNumber>.Success(new PhoneNationalNumber(normalizedNationalNumber));
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює національний номер зі збереженого
        /// канонічного значення.
        ///
        /// Збережене значення повинно містити лише ASCII-цифри
        /// без пробілів та інших символів форматування.
        ///
        /// (Restores a national number from its persisted canonical value.
        ///
        /// The persisted value must contain only ASCII digits
        /// without whitespace or other formatting characters.)
        /// </summary>
        /// <param name="storedNationalNumber">
        /// Збережене значення національного номера.
        ///
        /// (The persisted national-number value.)
        /// </param>
        /// <returns>
        /// Відновлений національний номер.
        ///
        /// (The restored national number.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення порушує правила номера
        /// або не відповідає канонічному формату.
        ///
        /// (Thrown when the persisted value violates national-number
        /// rules or does not match the canonical format.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        public static PhoneNationalNumber Restore(string storedNationalNumber)
        {
            var failure = NationalNumberValidator.ValidateForException(
                OperationType.Restore,
                storedNationalNumber
            );

            if (failure is not null)
            {
                throw failure.Value;
            }

            var normalizedNationalNumber = Normalize(storedNationalNumber);

            if (!string.Equals(
                storedNationalNumber,
                normalizedNationalNumber,
                StringComparison.Ordinal))
            {
                throw DomainDataInconsistencyException.InvalidFieldFormat<PhoneNationalNumber>(
                    nameof(PhoneNationalNumber.Value),
                    storedNationalNumber,
                    OperationType.Restore
                );
            }

            return new PhoneNationalNumber(storedNationalNumber);
        }
        #endregion

        /// <summary>
        /// Видаляє всі символи форматування та повертає
        /// послідовність ASCII-цифр.
        ///
        /// Метод повинен викликатися лише після успішної перевірки
        /// допустимих символів вхідного значення.
        ///
        /// (Removes all formatting characters and returns
        /// a sequence of ASCII digits.
        ///
        /// The method must be called only after successful validation
        /// of the permitted input characters.)
        /// </summary>
        /// <param name="value">
        /// Попередньо перевірене представлення національного номера.
        ///
        /// (The previously validated national-number representation.)
        /// </param>
        /// <returns>
        /// Канонічна послідовність ASCII-цифр.
        ///
        /// (The canonical sequence of ASCII digits.)
        /// </returns>
        private static string Normalize(string value)
        {
            return new string(
                value
                .Where(char.IsAsciiDigit)
                .ToArray()
            );
        }

        /// <inheritdoc/>
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
