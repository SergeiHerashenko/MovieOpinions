using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Errors;
using System.Text.RegularExpressions;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.DomainPart
{
    internal static partial class EmailDomainValidator
    {
        /// <summary>
        /// Перевіряє структурний формат доменної частини email-адреси:
        /// наявність доменних рівнів, допустимі символи, розташування
        /// дефісів і коректність домену верхнього рівня.
        ///
        /// Правило вимагає, щоб перевірка наявності значення
        /// правилом <see cref="RequiredRule"/> уже була успішною.
        ///
        /// (Validates the structural format of the email domain part,
        /// including domain labels, allowed characters, hyphen placement,
        /// and the top-level domain.
        ///
        /// This rule requires successful presence validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class RegexEmailDomainRule : IValidationRule<EmailDomainValidationData, ValidationFailure>
        {
            private static readonly Regex EmailDomainRegex =
                new(@"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,63}$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            public ValidationPriority Priority => ValidationPriority.Format;

            public ValidationFailure? Validate(EmailDomainValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<EmailDomainPart>(
                        nameof(RegexEmailDomainRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(EmailDomainPart.Value)
                        }
                    );
                }

                if (EmailDomainRegex.IsMatch(data.Value))
                    return null;

                return new ValidationFailure()
                {
                    Error = EmailDomainErrors.InvalidFormatEmailDomain<EmailDomainPart>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.InvalidFieldFormat<EmailDomainPart>(
                            nameof(EmailDomainPart.Value),
                            data.Value,
                            operationType
                        )
                };
            }
        }
    }
}
