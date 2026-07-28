using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Rules.DomainPart
{
    public sealed class NotAllowedDomainPartRule : IValidationRule<string, ValidationRestoreFailure>
    {
        private static readonly HashSet<string> BannedDomainsPart = new(StringComparer.OrdinalIgnoreCase)
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

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string clearDomain = value.Trim().ToLowerInvariant();

            if (!BannedDomainsPart.Contains(clearDomain))
                return null;

            return new ValidationRestoreFailure()
            {
                Error = EmailErrors.NotAllowedDomainPart<EmailDomainPart>(clearDomain),
                BuildException = () => DomainDataInconsistencyException.UnsupportedDiscriminator<EmailDomainPart>(nameof(value), value)
            };
        }
    }
}
