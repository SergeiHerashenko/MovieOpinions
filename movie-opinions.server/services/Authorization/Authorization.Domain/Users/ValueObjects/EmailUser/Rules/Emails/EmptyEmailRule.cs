using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Rules.Emails
{
    public class EmptyEmailRule : IValidationRule<string, ValidationRestoreFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Presence;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return null;

            return new ValidationRestoreFailure()
            {
                Error = EmailErrors.EmptyEmail<Email>(),
                BuildException = () => DomainDataInconsistencyException.Empty<Email>(nameof(value))
            };
        }
    }
}
