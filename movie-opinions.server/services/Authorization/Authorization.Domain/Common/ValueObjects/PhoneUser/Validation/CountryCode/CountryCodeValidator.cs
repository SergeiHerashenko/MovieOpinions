using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.CountryCode
{
    /// <summary>
    /// Координує правила синтаксичної валідації міжнародного
    /// телефонного коду та формує потрібне представлення
    /// першого виявленого порушення.
    ///
    /// Валідатор не перевіряє належність значення до переліку
    /// фактично призначених міжнародних телефонних кодів.
    ///
    /// (Coordinates syntactic validation rules for an international
    /// telephone country code and produces the required representation
    /// of the first detected failure.
    ///
    /// The validator does not verify that the value belongs to the list
    /// of assigned international telephone country codes.)
    /// </summary>
    internal static partial class CountryCodeValidator
    {
        /// <summary>
        /// Містить упорядкований набір правил валідації
        /// міжнародного телефонного коду.
        ///
        /// (Contains the ordered validation-rule set
        /// for an international telephone country code.)
        /// </summary>
        private static readonly ValidationOrchestrator<CountryCodeValidationData, ValidationFailure> _validation = new(
            [
                new RequiredRule(),
                new FormatCountryCodeRule(),
                new TooLongCountryCodeRule(),
                new TooShortCountryCodeRule()
            ]
        );

        /// <summary>
        /// Перевіряє телефонний код і повертає очікувану
        /// доменну помилку.
        ///
        /// (Validates a telephone country code and returns
        /// an expected domain error.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="value">
        /// Нормалізоване значення телефонного коду.
        ///
        /// (The normalized country calling code.)
        /// </param>
        /// <returns>
        /// Обгортка з першою виявленою помилкою або null,
        /// якщо всі правила виконані.
        ///
        /// (A wrapper containing the first detected error, or null if all rules succeed.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил.
        ///
        /// (Thrown when an internal precondition or the validation-rule execution order is violated.)
        /// </exception>
        internal static ValidationRulesFailure<Error>? ValidateForError(
            OperationType operationType,
            string value)
        {
            var data = BuildData(
                operationType,
                value
            );

            return DomainValidationExecutor.ValidateForError(
                _validation,
                data
            );
        }

        /// <summary>
        /// Перевіряє збережений телефонний код і для звичайного порушення
        /// створює відповідний об’єкт доменного винятку, не кидаючи його.
        ///
        /// Винятки порушення внутрішніх передумов правил
        /// передаються безпосередньо виклику.
        ///
        /// (Validates a persisted telephone country code and creates
        /// the corresponding domain exception object for a regular
        /// validation failure without throwing it.
        ///
        /// Internal rule-precondition exceptions propagate directly
        /// to the caller.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="value">
        /// Збережене значення телефонного коду.
        ///
        /// (The persisted country calling code.)
        /// </param>
        /// <returns>
        /// Обгортка зі створеним винятком або null,
        /// якщо всі правила виконані.
        ///
        /// (A wrapper containing the constructed exception, or null if all rules succeed.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил.
        ///
        /// (Thrown when an internal precondition or the validation-rule execution order is violated.)
        /// </exception>
        internal static ValidationRulesFailure<Exception>? ValidateForException(
            OperationType operationType,
            string value)
        {
            var data = BuildData(
                operationType,
                value
            );

            return DomainValidationExecutor.ValidateForException(
                _validation,
                data
            );
        }

        private static CountryCodeValidationData BuildData(
            OperationType operationType,
            string value)
        {
            return new CountryCodeValidationData()
            {
                OperationType = operationType,
                Value = value
            };
        }
    }
}
