using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.Emails
{
    internal static partial class EmailValidator
    {
        /// <summary>
        /// Перевіряє наявність обов’язкового значення email-адреси.
        ///
        /// Правило виконується першим і встановлює передумову
        /// для наступних правил формату та довжини.
        ///
        /// (Validates the presence of the required email-address value.
        ///
        /// This rule executes first and establishes the prerequisite
        /// for subsequent format and length rules.)
        /// </summary>
        private sealed class RequiredRule : IValidationRule<EmailValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Presence;

            public ValidationFailure? Validate(EmailValidationData data)
            {
                if (!string.IsNullOrWhiteSpace(data.Value))
                    return null;

                return new ValidationFailure()
                {
                    Error = EmailErrors.EmptyEmail<Email>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.Empty<Email>(
                            nameof(Email),
                            operationType
                        )
                };
            }
        }
    }
}
