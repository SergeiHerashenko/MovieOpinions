using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.DomainPart
{
    internal static partial class EmailDomainValidator
    {
        /// <summary>
        /// Перевіряє, що довжина доменної частини email-адреси
        /// не менша за встановлене мінімальне значення.
        ///
        /// Правило вимагає, щоб перевірка наявності значення
        /// правилом <see cref="RequiredRule"/> уже була успішною.
        ///
        /// (Validates that the email domain part satisfies
        /// the configured minimum length.
        ///
        /// This rule requires successful presence validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class TooShortEmailDomainRule : IValidationRule<EmailDomainValidationData, ValidationFailure>
        {
            private const int MIN_LENGTH_EMAIL_DOMAIN = 4;

            public ValidationPriority Priority => ValidationPriority.Length;

            public ValidationFailure? Validate(EmailDomainValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<EmailDomainPart>(
                        nameof(TooShortEmailDomainRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(EmailDomainPart.Value)
                        }
                    );
                }

                if (data.Value.Length >= MIN_LENGTH_EMAIL_DOMAIN)
                    return null;

                return new ValidationFailure()
                {
                    Error = EmailDomainErrors.TooShortEmailDomain<EmailDomainPart>(
                        data.Value.Length,
                        MIN_LENGTH_EMAIL_DOMAIN
                    ),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.ValueOutOfRange<EmailDomainPart>(
                            nameof(EmailDomainPart.Value),
                            data.Value.Length,
                            operationType,
                            context: new Dictionary<string, object>
                            {
                                ["ActualLength"] = data.Value.Length,
                                ["MinimumLength"] = MIN_LENGTH_EMAIL_DOMAIN
                            }
                        )
                };
            }
        }
    }
}
