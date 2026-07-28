using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Rules.CountryCode
{
    public sealed class EmptyCountryCodeRule : IValidationRule<string, ValidationRestoreFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Presence;

        public ValidationRestoreFailure? Validate(string value)
        {
            if(!string.IsNullOrWhiteSpace(value))
                return null;

            return new ValidationRestoreFailure()
            {
                Error = PhoneErrors.EmptyCountryCode<PhoneCountryCode>(),
                BuildException = () => DomainDataInconsistencyException.Empty<PhoneCountryCode>(nameof(value))
            };
        }
    }
}
