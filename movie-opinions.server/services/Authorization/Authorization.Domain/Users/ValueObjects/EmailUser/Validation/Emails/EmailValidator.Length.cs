using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.Emails
{
    internal static partial class EmailValidator
    {
        /// <summary>
        /// Перевіряє, що загальна довжина email-адреси
        /// не перевищує встановлене максимальне значення.
        ///
        /// Правило вимагає, щоб перевірка наявності значення
        /// правилом <see cref="RequiredRule"/> уже була успішною.
        ///
        /// (Validates that the complete email address does not exceed
        /// the configured maximum length.
        ///
        /// This rule requires successful presence validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class TooLongEmailRule : IValidationRule<EmailValidationData, ValidationFailure>
        {
            private const int MAX_LENGTH_EMAIL = 254;

            public ValidationPriority Priority => ValidationPriority.Length;

            public ValidationFailure? Validate(EmailValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<Email>(
                        nameof(TooLongEmailRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(Email)
                        }
                    );
                }

                if (data.Value.Length <= MAX_LENGTH_EMAIL)
                    return null;

                return new ValidationFailure()
                {
                    Error = EmailErrors.TooLongEmail<Email>(
                        data.Value.Length,
                        MAX_LENGTH_EMAIL
                    ),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.ValueOutOfRange<Email>(
                            nameof(Email),
                            data.Value.Length,
                            operationType,
                            context: new Dictionary<string, object>
                            {
                                ["ActualLength"] = data.Value.Length,
                                ["MaximumLength"] = MAX_LENGTH_EMAIL
                            }
                        )
                };
            }
        }
    }
}
