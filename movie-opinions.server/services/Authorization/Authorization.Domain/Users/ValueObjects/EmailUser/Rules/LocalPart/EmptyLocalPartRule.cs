using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Rules.LocalPart
{
    public sealed class EmptyLocalPartRule : IValidationRule<string, ValidationRestoreFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Presence;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return null;

            return new ValidationRestoreFailure()
            {
                Error = EmailErrors.EmptyLocalPart<EmailLocalPart>(),
                BuildException = () => DomainDataInconsistencyException.Empty<EmailLocalPart>(nameof(value))
            };
        }
    }
}
