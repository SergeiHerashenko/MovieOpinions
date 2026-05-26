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

        public static Result<CountryCode> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result<CountryCode>.Failure(PhoneError.EmptyCode);

            var trimmed = value.Trim();

            if (!trimmed.StartsWith("+"))
                return Result<CountryCode>.Failure(PhoneError.PhoneInvalidFormat);

            if (trimmed.Length < 2 || trimmed.Length > 5)
                return Result<CountryCode>.Failure(PhoneError.CountryCodeInvalidFormat);

            if (!trimmed.Skip(1).All(char.IsDigit))
                return Result<CountryCode>.Failure(PhoneError.CountryCodeInvalidFormat);

            return Result<CountryCode>.Success(new CountryCode(value));
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
