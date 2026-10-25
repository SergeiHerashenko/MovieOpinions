using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Errors;

namespace Authorization.Domain.Common.ValueObjects.EmailUser.Validation.Emails
{
    /// <summary>
    /// Координує правила загальної структури email-адреси
    /// та формує потрібне представлення першого виявленого порушення.
    ///
    /// Правила окремих локальної та доменної частин виконуються
    /// відповідними value objects після успішної загальної перевірки.
    ///
    /// (Coordinates the overall email-address validation rules
    /// and produces the required representation of the first failure.
    ///
    /// Rules specific to the local and domain parts are evaluated
    /// by the corresponding value objects after successful
    /// overall validation.)
    /// </summary>
    internal static partial class EmailValidator
    {
        /// <summary>
        /// Містить упорядкований набір правил загальної
        /// структури email-адреси.
        ///
        /// (Contains the ordered rule set for validating
        /// the overall email-address structure.)
        /// </summary>
        private static readonly ValidationOrchestrator<EmailValidationData, ValidationFailure> _validation = new(
            [
                new RequiredRule(),
                new WhitespaceEmailRule(),
                new SingleAtSignEmailRule(),
                new TooLongEmailRule()
            ]
        );

        /// <summary>
        /// Перевіряє email-адресу та повертає очікувану
        /// доменну помилку для операції створення.
        ///
        /// (Validates an email address and returns an expected
        /// domain error for a creation operation.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="value">
        /// Нормалізоване значення email-адреси.
        ///
        /// (The normalized email-address value.)
        /// </param>
        /// <returns>
        /// Обгортка з першою виявленою помилкою або null,
        /// якщо всі правила виконані.
        ///
        /// (A wrapper containing the first detected error,
        /// or null if all rules succeed.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил.
        ///
        /// (Thrown when an internal precondition or the
        /// validation-rule execution order is violated.)
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
        /// Перевіряє збережену email-адресу та для звичайного порушення
        /// створює відповідний об’єкт доменного винятку, не кидаючи його.
        ///
        /// Винятки, спричинені порушенням внутрішніх передумов правил,
        /// передаються безпосередньо виклику.
        ///
        /// (Validates a persisted email address and creates
        /// the corresponding domain exception object for a regular
        /// validation failure without throwing it.
        ///
        /// Exceptions caused by violated internal rule preconditions
        /// propagate directly to the caller.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="value">
        /// Збережене значення email-адреси.
        ///
        /// (The persisted email-address value.)
        /// </param>
        /// <returns>
        /// Обгортка зі створеним винятком або null,
        /// якщо всі правила виконані.
        ///
        /// (A wrapper containing the constructed exception,
        /// or null if all rules succeed.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил.
        ///
        /// (Thrown when an internal precondition or the
        /// validation-rule execution order is violated.)
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

        private static EmailValidationData BuildData(
            OperationType operationType,
            string value)
        {
            return new EmailValidationData()
            {
                OperationType = operationType,
                Value = value
            };
        }
    }
}
