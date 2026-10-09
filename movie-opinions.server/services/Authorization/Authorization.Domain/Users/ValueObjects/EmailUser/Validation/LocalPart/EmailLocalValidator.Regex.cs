using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Errors;
using System.Text.RegularExpressions;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.LocalPart
{
    internal static partial class EmailLocalValidator
    {
        /// <summary>
        /// Перевіряє структурний формат локальної частини email-адреси.
        ///
        /// Значення може містити латинські літери, цифри та роздільники
        /// '.', '_' або '-'. Роздільники не можуть розташовуватися
        /// на початку, у кінці або безпосередньо один після одного.
        ///
        /// Правило вимагає, щоб перевірка наявності значення
        /// правилом <see cref="RequiredRule"/> уже була успішною.
        ///
        /// (Validates the structural format of the email local part.
        ///
        /// The value may contain Latin letters, digits, and the separators
        /// '.', '_', or '-'. Separators cannot appear at the beginning,
        /// at the end, or directly next to one another.
        ///
        /// This rule requires successful presence validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class RegexEmailLocalRule : IValidationRule<EmailLocalValidationData, ValidationFailure>
        {
            private static readonly Regex EmailLocalRegex = new(
                @"^[a-zA-Z0-9]+(?:[._-][a-zA-Z0-9]+)*$",
                RegexOptions.Compiled
            );

            public ValidationPriority Priority => ValidationPriority.Format;

            public ValidationFailure? Validate(EmailLocalValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<EmailLocalPart>(
                        nameof(RegexEmailLocalRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(EmailLocalPart.Value)
                        }
                    );
                }

                if (EmailLocalRegex.IsMatch(data.Value))
                    return null;

                return new ValidationFailure()
                {
                    Error = EmailLocalErrors.InvalidFormatEmailLocal<EmailLocalPart>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.InvalidFieldFormat<EmailLocalPart>(
                            nameof(EmailLocalPart.Value),
                            data.Value,
                            operationType
                        )
                };
            }
        }
    }
}
