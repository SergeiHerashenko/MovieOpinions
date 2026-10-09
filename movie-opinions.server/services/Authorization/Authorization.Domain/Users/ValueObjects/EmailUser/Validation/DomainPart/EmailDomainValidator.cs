using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.DomainPart
{
    /// <summary>
    /// Координує структурну валідацію доменної частини email-адреси
    /// та політики, що застосовуються лише під час створення.
    ///
    /// Структурні правила використовуються як під час створення,
    /// так і під час відновлення. Політика заборонених доменів
    /// виконується лише під час створення і тільки після успішної
    /// структурної валідації.
    ///
    /// (Coordinates structural validation of the email domain part
    /// and policies applied only during creation.
    ///
    /// Structural rules are used during both creation and restoration.
    /// The blocked-domain policy is evaluated only during creation
    /// and only after successful structural validation.)
    /// </summary>
    internal static partial class EmailDomainValidator
    {
        /// <summary>
        /// Містить політики, які застосовуються лише до створення
        /// нових доменних частин email-адрес.
        ///
        /// (Contains policies applied only when creating
        /// new email domain parts.)
        /// </summary>
        private static readonly
            ValidationOrchestrator<EmailDomainValidationData, ValidationFailure> _creationPolicyValidation = new(
                [
                    new BlockedEmailDomainRule()
                ]
            );

        /// <summary>
        /// Містить структурні правила, спільні для створення
        /// та відновлення доменної частини.
        ///
        /// (Contains structural rules shared by creation
        /// and restoration of the domain part.)
        /// </summary>
        private static readonly
            ValidationOrchestrator<EmailDomainValidationData, ValidationFailure> _structuralValidation = new(
                [
                    new RequiredRule(),
                    new RegexEmailDomainRule(),
                    new TooLongEmailDomainRule(),
                    new TooShortEmailDomainRule()
                ]
            );

        /// <summary>
        /// Перевіряє нормалізоване значення під час створення.
        ///
        /// Спочатку виконує структурну валідацію, а після її
        /// успішного завершення — політики створення.
        ///
        /// (Validates a normalized value during creation.
        ///
        /// Structural validation is performed first, followed by
        /// creation policies after structural validation succeeds.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        /// </param>
        /// <param name="value">
        /// Нормалізоване значення доменної частини.
        /// </param>
        /// <returns>
        /// Обгортка з першою виявленою доменною помилкою
        /// або null, якщо всі правила виконані.
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

            var structuralFailure = DomainValidationExecutor.ValidateForError(
                _structuralValidation,
                data
            );

            if (structuralFailure is not null)
                return structuralFailure;

            return DomainValidationExecutor.ValidateForError(
                _creationPolicyValidation,
                data
            );
        }

        /// <summary>
        /// Перевіряє збережене значення за структурними правилами
        /// та створює відповідний об’єкт доменного винятку.
        ///
        /// Політики створення, зокрема список заборонених доменів,
        /// під час відновлення не застосовуються.
        ///
        /// Метод повертає об’єкт винятку для звичайного порушення
        /// валідності, але самостійно його не кидає. Винятки
        /// порушення внутрішніх передумов передаються безпосередньо виклику.
        ///
        /// (Validates a persisted value using structural rules
        /// and creates the corresponding domain exception object.
        ///
        /// Creation policies, including the blocked-domain list,
        /// are not applied during restoration.
        ///
        /// The method returns the exception object for a regular validation
        /// failure without throwing it. Internal precondition exceptions
        /// propagate directly to the caller.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        /// </param>
        /// <param name="value">
        /// Збережене значення доменної частини.
        /// </param>
        /// <returns>
        /// Обгортка зі створеним винятком або null,
        /// якщо структурні правила виконані.
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
                _structuralValidation,
                data
            );
        }

        /// <summary>
        /// Формує незмінний набір даних для виконання правил валідації.
        ///
        /// (Builds an immutable data set used by the validation rules.)
        /// </summary>
        private static EmailDomainValidationData BuildData(
            OperationType operationType,
            string value)
        {
            return new EmailDomainValidationData()
            {
                OperationType = operationType,
                Value = value
            };
        }
    }
}
