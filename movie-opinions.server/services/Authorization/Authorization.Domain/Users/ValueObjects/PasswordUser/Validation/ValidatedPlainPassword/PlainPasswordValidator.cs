using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Validation.ValidatedPlainPassword
{
    /// <summary>
    /// Координує правила перевірки пароля у відкритому вигляді
    /// та повертає очікувану доменну помилку.
    ///
    /// Валідатор використовується лише під час створення PlainPassword
    /// і не підтримує відновлення значення.
    ///
    /// (Coordinates plaintext-password validation rules
    /// and returns an expected domain error.
    ///
    /// The validator is used only when creating PlainPassword
    /// and does not support value restoration.)
    /// </summary>
    internal static partial class PlainPasswordValidator
    {
        private static readonly
            ValidationOrchestrator<PlainPasswordValidationData, ErrorValidationFailure> _validation = new(
                [
                    new RequiredRule(),
                    new LowercaseLetterRequiredRule(),
                    new UppercaseLetterRequiredRule(),
                    new DigitRequiredRule(),
                    new TooLongPlainPasswordRule(),
                    new TooShortPlainPasswordRule()
                ]
            );

        /// <summary>
        /// Перевіряє пароль та повертає перше виявлене порушення.
        ///
        /// Тип операції використовується як діагностичний контекст,
        /// якщо порушено внутрішню передумову правила.
        ///
        /// (Validates the password and returns the first detected violation.
        ///
        /// The operation type is used as diagnostic context
        /// when an internal rule precondition is violated.)
        /// </summary>
        /// <param name="operationType">Операція, під час якої виконується перевірка.</param>
        /// <param name="value">Пароль у відкритому вигляді.</param>
        /// <returns>
        /// Обгортка з очікуваною доменною помилкою
        /// або null, якщо всі правила виконані.
        /// </returns>
        internal static ValidationRulesFailure<Error>? ValidateForError(
            OperationType operationType,
            string value)
        {
            var data = new PlainPasswordValidationData()
            {
                OperationType = operationType,
                Value = value
            };

            return DomainValidationExecutor.ValidateForError(
                _validation,
                data
            );
        }

        /// <summary>
        /// Створює виняток, що описує порушення внутрішньої
        /// передумови правила валідації.
        ///
        /// (Creates an exception describing a violated internal
        /// validation-rule precondition.)
        /// </summary>
        private static DomainInvalidOperationException CreatePreconditionException(
            string sourceContext,
            OperationType operationType)
        {
            return DomainInvalidOperationException.PreconditionFailed<PlainPassword>(
                sourceContext,
                nameof(RequiredRule),
                operationType,
                context: new Dictionary<string, object>
                {
                    ["FieldName"] = nameof(PlainPassword.Value)
                }
            );
        }
    }
}
