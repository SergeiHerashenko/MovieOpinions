using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Rules.CountryCode
{
    public sealed class TooLongCountryCodeRule : IValidationRule<string, ValidationRestoreFailure>
    {
        private const int MAX_LENGTH_PHONE_COUNTRY_CODE = 5;

        public ValidationPriority Priority => ValidationPriority.Length;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.Length < MAX_LENGTH_PHONE_COUNTRY_CODE)
                return null;

            return new ValidationRestoreFailure()
            {
                Error = PhoneErrors.TooLongCountryCode<PhoneCountryCode>(value),
                BuildException = () => DomainDataInconsistencyException.ValueOutOfRange<PhoneCountryCode>(nameof(value), value)
            };
        }
    }
}
