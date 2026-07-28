using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Rules.CountryCode
{
    public sealed class TooShortCountryCodeRule : IValidationRule<string, ValidationRestoreFailure>
    {
        private const int MIN_LENGTH_PHONE_COUNTRY_CODE = 2;

        public ValidationPriority Priority => ValidationPriority.Length;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.Length >= MIN_LENGTH_PHONE_COUNTRY_CODE)
                return null;

            return new ValidationRestoreFailure()
            {
                Error = PhoneErrors.TooShortCountryCode<PhoneCountryCode>(value),
                BuildException = () => DomainDataInconsistencyException.ValueOutOfRange<PhoneCountryCode>(nameof(value), value)
            };
        }
    }
}
