using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.Emails
{
    internal static partial class EmailValidator
    {
        /// <summary>
        /// Перевіряє відсутність пробільних символів
        /// усередині нормалізованої email-адреси.
        ///
        /// Правило вимагає, щоб перевірка наявності значення
        /// правилом <see cref="RequiredRule"/> уже була успішною.
        ///
        /// (Validates that a normalized email address contains
        /// no whitespace characters.
        ///
        /// This rule requires successful presence validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class WhitespaceEmailRule : IValidationRule<EmailValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Format;

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
