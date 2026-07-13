using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser
{
    public sealed class CountryCode : ValueObject
    {
        public string Value { get; }

        private CountryCode(string value)
        {
            Value = value;
        }

        public static Result<CountryCode> Create(string rawCountryCode)
        {
            if (string.IsNullOrWhiteSpace(rawCountryCode))
                return Result<CountryCode>.Failure(UserErrors.PhoneError.EmptyCountryCode<CountryCode>());

            var trimmed = rawCountryCode.Trim();

            if (!trimmed.StartsWith("+"))
                return Result<CountryCode>.Failure(UserErrors.PhoneError.CountryCodeInvalidFormat<CountryCode>());

            if (trimmed.Length < 3 || trimmed.Length > 5)
                return Result<CountryCode>.Failure(UserErrors.PhoneError.CountryCodeInvalidFormat<CountryCode>());

            if (!trimmed.Skip(1).All(char.IsDigit))
                return Result<CountryCode>.Failure(UserErrors.PhoneError.CountryCodeInvalidFormat<CountryCode>());

            return Result<CountryCode>.Success(new CountryCode(trimmed));
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
