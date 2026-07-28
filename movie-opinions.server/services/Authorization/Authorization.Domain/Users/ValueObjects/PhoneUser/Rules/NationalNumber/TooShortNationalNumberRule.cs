using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Rules.NationalNumber
{
    public sealed class TooShortNationalNumberRule : IValidationRule<string, ValidationRestoreFailure>
    {
        private const int MIN_LENGTH_PHONE_NATIONAL_NUMBER = 7;

        public ValidationPriority Priority => ValidationPriority.Length;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            int digitsCount = value.Count(char.IsDigit);

            if (digitsCount >= MIN_LENGTH_PHONE_NATIONAL_NUMBER)
                return null;

            return new ValidationRestoreFailure()
            {
                Error = PhoneErrors.TooShortNationalNumber<PhoneNationalNumber>(value),
                BuildException = () => DomainDataInconsistencyException.ValueOutOfRange<PhoneNationalNumber>(nameof(value), value)
            };
        }
    }
}
