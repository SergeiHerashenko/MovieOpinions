using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Common.Validation.Interfaces;
using Authorization.Domain.Common.ValueObjects.EmailUser.Errors;

namespace Authorization.Domain.Common.ValueObjects.EmailUser.Validation.DomainPart
{
    internal static partial class EmailDomainValidator
    {
        /// <summary>
        /// Перевіряє наявність обов’язкового значення
        /// доменної частини email-адреси.
        ///
        /// Правило виконується першим і забезпечує передумову
        /// для наступних структурних правил.
        ///
        /// (Validates the presence of the required email domain value.
        ///
        /// This rule executes first and establishes the prerequisite
        /// required by subsequent structural rules.)
        /// </summary>
        private sealed class RequiredRule : IValidationRule<EmailDomainValidationData, ValidationFailure>
        {
            /// <inheritdoc/>
            public ValidationPriority Priority => ValidationPriority.Presence;

            /// <inheritdoc/>
            public ValidationFailure? Validate(EmailDomainValidationData data)
            {
                if (!string.IsNullOrWhiteSpace(data.Value))
                {
                    return null;
                }

                return new ValidationFailure()
                {
                    Error = EmailDomainErrors.EmptyEmailDomain<EmailDomainPart>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.Empty<EmailDomainPart>(
                            nameof(EmailDomainPart.Value),
                            operationType
                        )
                };
            }
        }
    }
}
