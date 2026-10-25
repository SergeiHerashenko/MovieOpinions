using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Common.Validation.Interfaces;
using Authorization.Domain.Common.ValueObjects.EmailUser.Errors;

namespace Authorization.Domain.Common.ValueObjects.EmailUser.Validation.Emails
{
    internal static partial class EmailValidator
    {
        /// <summary>
        /// Перевіряє, що email-адреса не містить пробільних символів.
        ///
        /// Правило вимагає, щоб перевірка наявності значення
        /// правилом <see cref="RequiredRule"/> уже була успішною.
        ///
        /// (Validates that an email address contains no whitespace characters.
        ///
        /// This rule requires successful presence validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class WhitespaceEmailRule : IValidationRule<EmailValidationData, ValidationFailure>
        {
            /// <inheritdoc/>
            public ValidationPriority Priority => ValidationPriority.Format;

            /// <inheritdoc/>
            public ValidationFailure? Validate(EmailValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<Email>(
                        nameof(WhitespaceEmailRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(Email)
                        }
                    );
                }

                if (!data.Value.Any(char.IsWhiteSpace))
                    return null;

                return new ValidationFailure()
                {
                    Error = EmailErrors.InvalidFormatEmail<Email>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.InvalidFieldFormat<Email>(
                            nameof(Email),
                            data.Value,
                            operationType
                        )
                };
            }
        }
    }
}
