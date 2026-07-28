using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Rules.NationalNumber;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser
{
    public sealed class PhoneNationalNumber : ValueObject
    {
        public string Value { get; }

        private PhoneNationalNumber(string value)
        {
            Value = value;
        }

        private static readonly ValidationOrchestrator<string, ValidationRestoreFailure> _validator = new(
            [
                new EmptyNationalNumberRule(),
                new FormatNationalNumberRule(),
                new TooLongNationalNumberRule(),
                new TooShortNationalNumberRule()
            ]
        );

        #region Creation
        public static Result<PhoneNationalNumber> Create(string rawNationalNumber)
        {
            var failure = _validator.Validate(rawNationalNumber);

            if (failure is not null)
                return Result<PhoneNationalNumber>.Failure(failure.Error);

            var trimmed = rawNationalNumber.Trim();

            return Result<PhoneNationalNumber>.Success(new PhoneNationalNumber(trimmed));
        }
        #endregion

        #region Restoration
        public static PhoneNationalNumber Restore(string rawNationalNumber)
        {
            var failure = _validator.Validate(rawNationalNumber);

            if (failure is not null)
                throw failure.BuildException();

            var trimmed = rawNationalNumber.Trim();

            return new PhoneNationalNumber(trimmed);
        }
        #endregion

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
