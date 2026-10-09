using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.DomainPart
{
    internal static partial class EmailDomainValidator
    {
        /// <summary>
        /// Перевіряє політику заборонених email-доменів,
        /// що застосовується лише під час створення.
        ///
        /// Правило повинно виконуватися тільки після успішного
        /// завершення структурної валідації доменної частини.
        ///
        /// (Validates the blocked email-domain policy applied
        /// only during creation.
        ///
        /// This rule must execute only after successful structural
        /// validation of the domain part.)
        /// </summary>
        private sealed class BlockedEmailDomainRule : IValidationRule<EmailDomainValidationData, ValidationFailure>
        {
            /// <summary>
            /// Нечутливий до регістру набір доменів,
            /// заборонених для нових email-адрес.
            ///
            /// (Case-insensitive set of domains blocked
            /// for newly created email addresses.)
            /// </summary>
            private static readonly HashSet<string> BlockedDomains = new(StringComparer.OrdinalIgnoreCase)
            {
                "mail.ru",
                "yandex.ru",
                "bk.ru",
                "inbox.ru",
                "list.ru",
                "rambler.ru",
                "internet.ru",
                "xmail.ru"
            };

            public ValidationPriority Priority => ValidationPriority.BusinessRule;

            public ValidationFailure? Validate(EmailDomainValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<EmailDomainPart>(
                        nameof(BlockedEmailDomainRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(EmailDomainPart.Value)
                        }
                    );
                }

                if (!BlockedDomains.Contains(data.Value))
                    return null;

                return new ValidationFailure()
                {
                    Error = EmailDomainErrors.NotAllowedEmailDomain<EmailDomainPart>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.UnsupportedDiscriminator<EmailDomainPart>(
                            nameof(EmailDomainPart.Value),
                            data.Value,
                            operationType
                        )
                };
            }
        }
    }
}
