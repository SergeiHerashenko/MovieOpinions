using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.LocalPart
{
    internal static partial class EmailLocalValidator
    {
        /// <summary>
        /// Перевіряє наявність обов’язкового значення локальної
        /// частини email-адреси.
        ///
        /// Правило виконується першим і встановлює передумову
        /// для наступних правил формату та довжини.
        ///
        /// (Validates the presence of the required email local-part value.
        ///
        /// This rule executes first and establishes the prerequisite
        /// for subsequent format and length rules.)
        /// </summary>
        private sealed class RequiredRule : IValidationRule<EmailLocalValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Presence;

            public ValidationFailure? Validate(EmailLocalValidationData data)
            {
                if (!string.IsNullOrWhiteSpace(data.Value))
                    return null;

                return new ValidationFailure()
                {
                    Error = EmailLocalErrors.EmptyEmailLocal<EmailLocalPart>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.Empty<EmailLocalPart>(
                            nameof(EmailLocalPart.Value),
                            operationType
                        )
                };
            }
        }
    }
}
