using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.CountryCode;
using Authorization.Domain.Results;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser
{
    /// <summary>
    /// Представляє міжнародний телефонний код країни у форматі
    /// символу '+' та від однієї до трьох ASCII-цифр.
    ///
    /// Перша цифра коду не може бути нулем. Value object перевіряє
    /// лише синтаксичну коректність, але не належність коду
    /// до реєстру фактично призначених міжнародних кодів.
    ///
    /// (Represents an international telephone country code consisting
    /// of a '+' character followed by one to three ASCII digits.
    ///
    /// The first digit cannot be zero. The value object validates
    /// syntactic correctness but does not verify that the code is present
    /// in the registry of assigned international calling codes.)
    /// </summary>
    public sealed class PhoneCountryCode : ValueObject
    {
        /// <summary>
        /// Нормалізоване значення міжнародного телефонного коду.
        ///
        /// (Normalized international telephone country-code value.)
        /// </summary>
        public string Value { get; }

        private PhoneCountryCode(string value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Нормалізує зовнішнє значення та створює міжнародний
        /// телефонний код після успішної валідації.
        ///
        /// (Normalizes an external value and creates an international
        /// telephone country code after successful validation.)
        /// </summary>
        /// <param name="rawCountryCode">
        /// Зовнішнє значення телефонного коду країни.
        /// Перед перевіркою видаляються початкові та кінцеві пробільні символи.
        /// null перетворюється на порожній рядок.
        ///
        /// (The external country-code value.
        /// Leading and trailing whitespace is removed before validation.
        /// null is converted to an empty string.)
        /// </param>
        /// <returns>
        /// Успішний результат із телефонним кодом або перша
        /// виявлена помилка валідації.
        ///
        /// (A successful result containing the country calling
        /// code, or the first validation error.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        public static Result<PhoneCountryCode> Create(string rawCountryCode)
        {
            var normalizedCountryCode = rawCountryCode?.Trim()
                ?? string.Empty;

            var failure = CountryCodeValidator.ValidateForError(
                OperationType.Create,
                normalizedCountryCode
            );

            if (failure is not null)
                return Result<PhoneCountryCode>.Failure(failure.Value);

            return Result<PhoneCountryCode>.Success(new PhoneCountryCode(normalizedCountryCode));
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює міжнародний телефонний код країни
        /// зі збереженого значення.
        ///
        /// (Restores an international telephone country code
        /// from its persisted value.)
        /// </summary>
        /// <param name="storedCountryCode">
        /// Збережене значення телефонного коду країни.
        ///
        /// (The persisted country calling code.)
        /// </param>
        /// <returns>
        /// Відновлений телефонний код країни.
        ///
        /// (The restored country calling code.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення порушує правила
        /// телефонного коду країни.
        ///
        /// (Thrown when the persisted value violates
        /// the country calling code validation rules.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
        /// </exception>
        public static PhoneCountryCode Restore(string storedCountryCode)
        {
            var failure = CountryCodeValidator.ValidateForException(
                OperationType.Restore,
                storedCountryCode
            );

            if (failure is not null)
                throw failure.Value;

            return new PhoneCountryCode(storedCountryCode);
        }
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
