using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Rules.DomainPart
{
    public sealed class TooShortDomainPartRule : IValidationRule<string, ValidationRestoreFailure>
    {
        private const int MIN_LENGTH_EMAIL_DOMAIN = 4;

        public ValidationPriority Priority => ValidationPriority.Length;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.Length >= MIN_LENGTH_EMAIL_DOMAIN)
                return null;

            return new ValidationRestoreFailure()
            {
                Error = EmailErrors.TooShortDomainPart<EmailDomainPart>(value),
                BuildException = () => DomainDataInconsistencyException.ValueOutOfRange<EmailDomainPart>(nameof(value), value)
            };
        }
    }
}
