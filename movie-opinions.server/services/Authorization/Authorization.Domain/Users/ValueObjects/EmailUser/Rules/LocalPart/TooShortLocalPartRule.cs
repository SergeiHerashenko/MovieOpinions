using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Rules.LocalPart
{
    public sealed class TooShortLocalPartRule : IValidationRule<string, ValidationRestoreFailure>
    {
        private const int MIN_LENGTH_EMAIL_LOCAL_PART = 2;

        public ValidationPriority Priority => ValidationPriority.Length;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.Length > MIN_LENGTH_EMAIL_LOCAL_PART)
                return null;

            return new ValidationRestoreFailure()
            {
                Error = EmailErrors.TooShortLocalPart<EmailLocalPart>(value),
                BuildException = () => DomainDataInconsistencyException.ValueOutOfRange<EmailLocalPart>(nameof(value), value)
            };
        }
    }
}
