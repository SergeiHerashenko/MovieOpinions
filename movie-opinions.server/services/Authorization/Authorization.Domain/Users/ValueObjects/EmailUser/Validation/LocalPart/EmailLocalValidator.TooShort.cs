using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.LocalPart
{
    internal static partial class EmailLocalValidator
    {
        /// <summary>
        /// Перевіряє, що довжина локальної частини email-адреси
        /// не менша за встановлене мінімальне значення.
        ///
        /// Правило вимагає, щоб перевірка наявності значення
        /// правилом <see cref="RequiredRule"/> уже була успішною.
        ///
        /// (Validates that the email local part satisfies
        /// the configured minimum length.
        ///
        /// This rule requires successful presence validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class TooShortEmailLocalRule : IValidationRule<EmailLocalValidationData, ValidationFailure>
        {
            private const int MIN_LENGTH_EMAIL_LOCAL_PART = 2;

            public ValidationPriority Priority => ValidationPriority.Length;

            public ValidationFailure? Validate(EmailLocalValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<EmailLocalPart>(
                        nameof(TooShortEmailLocalRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(EmailLocalPart.Value)
                        }
                    );
                }

                if (data.Value.Length >= MIN_LENGTH_EMAIL_LOCAL_PART)
                    return null;

                return new ValidationFailure()
                {
                    Error = EmailLocalErrors.TooShortEmailLocal<EmailLocalPart>(
                        data.Value.Length,
                        MIN_LENGTH_EMAIL_LOCAL_PART
                    ),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.ValueOutOfRange<EmailLocalPart>(
                            nameof(EmailLocalPart.Value),
                            data.Value.Length,
                            operationType,
                            context: new Dictionary<string, object>
                            {
                                ["ActualLength"] = data.Value.Length,
                                ["MinimumLength"] = MIN_LENGTH_EMAIL_LOCAL_PART
                            }
                        )
                };
            }
        }
    }
}
