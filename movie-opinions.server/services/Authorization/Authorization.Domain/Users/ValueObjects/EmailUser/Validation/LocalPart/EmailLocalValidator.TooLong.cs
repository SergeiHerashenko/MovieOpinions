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
        /// не перевищує встановлене максимальне значення.
        ///
        /// Правило вимагає, щоб перевірка наявності значення
        /// правилом <see cref="RequiredRule"/> уже була успішною.
        ///
        /// (Validates that the email local part does not exceed
        /// the configured maximum length.
        ///
        /// This rule requires successful presence validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class TooLongEmailLocalRule : IValidationRule<EmailLocalValidationData, ValidationFailure>
        {
            private const int MAX_LENGTH_EMAIL_LOCAL_PART = 50;

            public ValidationPriority Priority => ValidationPriority.Length;

            public ValidationFailure? Validate(EmailLocalValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<EmailLocalPart>(
                        nameof(TooLongEmailLocalRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(EmailLocalPart.Value)
                        }
                    );
                }

                if (data.Value.Length <= MAX_LENGTH_EMAIL_LOCAL_PART)
                    return null;

                return new ValidationFailure()
                {
                    Error = EmailLocalErrors.TooLongEmailLocal<EmailLocalPart>(
                        data.Value.Length,
                        MAX_LENGTH_EMAIL_LOCAL_PART
                    ),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.ValueOutOfRange<EmailLocalPart>(
                            nameof(EmailLocalPart.Value),
                            data.Value.Length,
                            operationType,
                            context: new Dictionary<string, object>
                            {
                                ["ActualLength"] = data.Value.Length,
                                ["MaximumLength"] = MAX_LENGTH_EMAIL_LOCAL_PART
                            }
                        )
                };
            }
        }
    }
}
