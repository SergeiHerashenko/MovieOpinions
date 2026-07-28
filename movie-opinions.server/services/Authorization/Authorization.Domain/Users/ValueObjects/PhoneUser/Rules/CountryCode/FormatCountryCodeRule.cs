using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Rules.CountryCode
{
    public sealed class FormatCountryCodeRule : IValidationRule<string, ValidationRestoreFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Format;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!value.StartsWith("+"))
                return CreateFailure(value);

            if(!value.Skip(1).All(char.IsDigit))
                return CreateFailure(value);

            return null;
        }

        private static ValidationRestoreFailure CreateFailure(string value)
        {
            return new ValidationRestoreFailure()
            {
                Error = PhoneErrors.InvalidFormatCountryCode<PhoneCountryCode>(value),
                BuildException = () => DomainDataInconsistencyException.InvalidFieldFormat<PhoneCountryCode>(nameof(value), value)
            };
        }
    }
}
