using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Rules.CountryCode;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser
{
    public sealed class PhoneCountryCode : ValueObject
    {
        public string Value { get; }

        private PhoneCountryCode(string value)
        {
            Value = value;
        }

        private static readonly ValidationOrchestrator<string, ValidationRestoreFailure> _validator = new(
            [
                new EmptyCountryCodeRule(),
                new TooShortCountryCodeRule(),
                new TooLongCountryCodeRule(),
                new FormatCountryCodeRule()
            ]
        );

        #region Creation
        public static Result<PhoneCountryCode> Create(string rawCountryCode)
        {
            var failure = _validator.Validate(rawCountryCode);

            if (failure is not null)
                return Result<PhoneCountryCode>.Failure(failure.Error);

            var trimmed = rawCountryCode.Trim();

            return Result<PhoneCountryCode>.Success(new PhoneCountryCode(trimmed));
        }
        #endregion

        #region Restoration
        public static PhoneCountryCode Restore(string rawCountryCode)
        {
            var failure = _validator.Validate(rawCountryCode);

            if (failure is not null)
                throw failure.BuildException();

            var trimmed = rawCountryCode.Trim();

            return new PhoneCountryCode(trimmed);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
