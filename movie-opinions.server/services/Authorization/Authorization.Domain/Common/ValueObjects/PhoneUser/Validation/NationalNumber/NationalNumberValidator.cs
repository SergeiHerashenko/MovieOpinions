using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.NationalNumber
{
    internal static partial class NationalNumberValidator
    {
        /// <summary>
        /// Містить упорядкований набір правил валідації
        /// національного телефонного номера.
        ///
        /// (Contains the ordered national telephone-number
        /// validation-rule set.)
        /// </summary>
        private static readonly
            ValidationOrchestrator<NationalNumberValidationData, ValidationFailure> _validation = new(
                [
                    new RequiredRule(),
                    new FormatNationalNumberRule(),
                    new TooLongNationalNumberRule(),
                    new TooShortNationalNumberRule()
                ]
            );

        /// <summary>
        /// Перевіряє національний номер і повертає
        /// очікувану доменну помилку.
        ///
        /// (Validates a national number and returns
        /// an expected domain error.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="value">
        /// Зовнішнє представлення національного номера.
        ///
        /// (The external national-number representation.)
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
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
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
        /// Перевіряє збережений національний номер і для звичайного
        /// порушення створює об’єкт доменного винятку, не кидаючи його.
        ///
        /// Винятки порушення внутрішніх передумов правил
        /// передаються безпосередньо виклику.
        ///
        /// (Validates a persisted national number and creates a domain
        /// exception object for a regular validation failure without throwing it.
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
        /// Збережене значення національного номера.
        ///
        /// (The persisted national-number value.)
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
        /// (Thrown when an internal precondition or the validation-rule
        /// execution order is violated.)
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

        private static NationalNumberValidationData BuildData(
            OperationType operationType,
            string value)
        {
            return new NationalNumberValidationData()
            {
                OperationType = operationType,
                Value = value
            };
        }
    }
}
