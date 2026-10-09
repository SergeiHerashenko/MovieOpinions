using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Validation.CountryCode;
using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser
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
        /// Значення не повинно бути null.
        /// </param>
        /// <returns>
        /// Успішний результат із телефонним кодом або перша
        /// виявлена помилка валідації.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
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
        /// </param>
        /// <returns>Відновлений телефонний код країни.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення порушує правила
        /// телефонного коду країни.
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
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

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
