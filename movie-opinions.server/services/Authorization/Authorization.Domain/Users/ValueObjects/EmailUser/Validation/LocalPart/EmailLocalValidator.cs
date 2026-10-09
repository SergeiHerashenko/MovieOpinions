using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.LocalPart
{
    /// <summary>
    /// Координує повний набір правил валідації локальної
    /// частини email-адреси та формує потрібне представлення
    /// першого виявленого порушення.
    ///
    /// Один набір структурних правил використовується як під час
    /// створення, так і під час відновлення значення.
    ///
    /// (Coordinates the complete validation rule set for the email
    /// local part and produces the required representation
    /// of the first detected failure.
    ///
    /// The same structural rule set is used during both creation
    /// and restoration.)
    /// </summary>
    internal static partial class EmailLocalValidator
    {
        /// <summary>
        /// Містить упорядкований набір структурних правил
        /// локальної частини email-адреси.
        ///
        /// (Contains the ordered structural validation rules
        /// for the email local part.)
        /// </summary>
        private static readonly ValidationOrchestrator<EmailLocalValidationData, ValidationFailure> _validation = new(
            [
                new RequiredRule(),
                new RegexEmailLocalRule(),
                new TooLongEmailLocalRule(),
                new TooShortEmailLocalRule()
            ]
        );

        /// <summary>
        /// Перевіряє значення та повертає очікувану доменну помилку
        /// для операції створення.
        ///
        /// (Validates the value and returns an expected domain error
        /// for a creation operation.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        /// </param>
        /// <param name="value">
        /// Нормалізоване значення локальної частини.
        /// </param>
        /// <returns>
        /// Обгортка з першою виявленою помилкою або null,
        /// якщо всі правила виконані.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил.
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
        /// Перевіряє збережене значення та для звичайного порушення
        /// створює відповідний об’єкт доменного винятку, не кидаючи його.
        ///
        /// Винятки, спричинені порушенням внутрішніх передумов правил,
        /// передаються безпосередньо виклику.
        ///
        /// (Validates a persisted value and creates the corresponding
        /// domain exception object for a regular validation failure
        /// without throwing it.
        ///
        /// Exceptions caused by violated internal rule preconditions
        /// propagate directly to the caller.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        /// </param>
        /// <param name="value">
        /// Збережене значення локальної частини.
        /// </param>
        /// <returns>
        /// Обгортка зі створеним винятком або null,
        /// якщо всі правила виконані.
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил.
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

        private static EmailLocalValidationData BuildData(
            OperationType operationType,
            string value)
        {
            return new EmailLocalValidationData()
            {
                OperationType = operationType,
                Value = value
            };
        }
    }
}
