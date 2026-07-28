using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Rules.LocalPart
{
    public sealed class TooLongLocalPartRule : IValidationRule<string, ValidationRestoreFailure>
    {
        private const int MAX_LENGTH_EMAIL_LOCAL_PART = 50;

        public ValidationPriority Priority => ValidationPriority.Length;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.Length < MAX_LENGTH_EMAIL_LOCAL_PART)
                return null;

            return new ValidationRestoreFailure()
            {
                Error = EmailErrors.TooLongLocalPart<EmailLocalPart>(value),
                BuildException = () => DomainDataInconsistencyException.ValueOutOfRange<EmailLocalPart>(nameof(value), value)
            };
        }
    }
}
