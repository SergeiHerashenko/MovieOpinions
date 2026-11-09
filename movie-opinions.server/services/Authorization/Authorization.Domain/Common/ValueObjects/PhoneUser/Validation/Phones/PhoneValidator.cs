using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.Phones
{
    /// <summary>
    /// Координує структурну валідацію повного телефонного номера
    /// та політики, що застосовуються лише під час створення.
    ///
    /// Структурні правила використовуються під час створення
    /// і відновлення. Політика заборонених номерів виконується
    /// лише під час створення та після успішної структурної перевірки.
    ///
    /// (Coordinates structural validation of a complete telephone number
    /// and policies applied only during creation.
    ///
    /// Structural rules are used during both creation and restoration.
    /// The blocked-number policy executes only during creation
    /// and only after successful structural validation.)
    /// </summary>
    internal static partial class PhoneValidator
    {
        /// <summary>
        /// Містить політики, що застосовуються лише під час
        /// створення нового телефонного номера.
        ///
        /// (Contains policies applied only when creating
        /// a new telephone number.)
        /// </summary>
        private static readonly
            ValidationOrchestrator<PhoneValidationData, ValidationFailure> _creationPolicyValidation = new(
                [
                    new NotAllowedPhoneRule()
                ]
            );

        /// <summary>
        /// Містить структурні правила, спільні для створення
        /// та відновлення телефонного номера.
        ///
        /// (Contains structural rules shared by telephone-number
        /// creation and restoration.)
        /// </summary>
        private static readonly
            ValidationOrchestrator<PhoneValidationData, ValidationFailure> _structuralValidation = new(
                [
                    new RequiredRule(),
                    new NumberingPlanPhoneRule()
                ]
            );

        /// <summary>
        /// Перевіряє телефонний номер під час створення.
        ///
        /// Спочатку виконує структурну валідацію, а після її
        /// успішного завершення — політики створення.
        ///
        /// (Validates a telephone number during creation.
        ///
        /// Structural validation is performed first, followed by
        /// creation policies after structural validation succeeds.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="countryCode">
        /// Канонічний код країни.
        ///
        /// (The canonical country calling code.)
        /// </param>
        /// <param name="nationalNumber">
        /// Канонічна національна частина телефонного номера.
        ///
        /// (The canonical national part of the telephone number.)
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
        /// <exception cref="DomainInvariantViolationException">
        /// У каталозі телефонної нумерації знайдено записи
        /// з переданим міжнародним кодом, але їхній об’єднаний
        /// набір допустимих довжин національної частини порожній.
        ///
        /// Це означає порушення внутрішньої узгодженості каталогу.
        ///
        /// (The phone numbering plan catalog contains entries
        /// matching the supplied country calling code, but their
        /// combined set of allowed national number lengths is empty.
        ///
        /// This indicates an internal consistency violation in the catalog.)
        /// </exception>
        internal static ValidationRulesFailure<Error>? ValidateForError(
            OperationType operationType,
            string countryCode,
            string nationalNumber)
        {
            var data = BuildData(
                operationType,
                countryCode,
                nationalNumber
            );

            var structuralFailure = DomainValidationExecutor.ValidateForError(
                _structuralValidation,
                data
            );

            if (structuralFailure is not null)
            {
                return structuralFailure;
            }

            return DomainValidationExecutor.ValidateForError(
                _creationPolicyValidation,
                data
            );
        }

        /// <summary>
        /// Перевіряє структурну коректність відновленого телефонного
        /// номера та створює відповідний об’єкт доменного винятку.
        ///
        /// Політики створення під час відновлення не застосовуються.
        /// Об’єкт винятку для виявленого порушення повертається в обгортці.
        /// Винятки через порушення внутрішніх передумов не перехоплюються.
        ///
        /// (Validates the structural correctness of a restored telephone
        /// number and creates the corresponding domain exception object.
        ///
        /// Creation policies are not applied during restoration.
        /// The exception object for a detected violation is returned in a wrapper.
        /// Exceptions caused by internal precondition failures are not intercepted.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="countryCode">
        /// Збережений код країни.
        ///
        /// (The persisted country calling code.)
        /// </param>
        /// <param name="nationalNumber">
        /// Збережена національна частина.
        ///
        /// (The persisted national part of the number.)
        /// </param>
        /// <returns>
        /// Обгортка зі створеним винятком або null,
        /// якщо структурні правила виконані.
        ///
        /// (A wrapper containing the constructed exception,
        /// or null if all structural rules succeed.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил.
        ///
        /// (Thrown when an internal precondition
        /// or the validation-rule execution order is violated.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// У каталозі телефонної нумерації знайдено записи
        /// з переданим міжнародним кодом, але їхній об’єднаний
        /// набір допустимих довжин національної частини порожній.
        ///
        /// Це означає порушення внутрішньої узгодженості каталогу.
        ///
        /// (The phone numbering plan catalog contains entries
        /// matching the supplied country calling code, but their
        /// combined set of allowed national number lengths is empty.
        ///
        /// This indicates an internal consistency violation in the catalog.)
        /// </exception>
        internal static ValidationRulesFailure<Exception>? ValidateForException(
            OperationType operationType,
            string countryCode,
            string nationalNumber)
        {
            var data = BuildData(
                operationType,
                countryCode,
                nationalNumber
            );

            return DomainValidationExecutor.ValidateForException(
                _structuralValidation,
                data
            );
        }

        private static PhoneValidationData BuildData(
            OperationType operationType,
            string countryCode,
            string nationalNumber)
        {
            return new PhoneValidationData()
            {
                OperationType = operationType,
                CountryCode = countryCode,
                NationalNumber = nationalNumber
            };
        }
    }
}
