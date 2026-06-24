using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser
{
    public sealed class Phone : ValueObject
    {
        public CountryCode CountryCode { get; }

        public string Value { get; }

        private const int MIN_LENGTH_PHONE_NUMBER = 4;

        private const int MAX_LENGTH_PHONE_NUMBER = 14;

        private Phone(CountryCode countryCode, string value)
        {
            CountryCode = countryCode;
            Value = value;
        }

        #region Creation
        public static DomainResult<Phone> Create(CountryCode code, string rawPhone)
        {
            if (code is null)
                return DomainResult<Phone>.Failure(PhoneError.EmptyCode);

            if (string.IsNullOrWhiteSpace(rawPhone))
                return DomainResult<Phone>.Failure(PhoneError.EmptyPhone);

            var cleanedPhone = CleanPhoneNumber(rawPhone);

            if (string.IsNullOrEmpty(cleanedPhone))
                return DomainResult<Phone>.Failure(PhoneError.PhoneInvalidFormat);

            var isValid = IsValidPhoneNumber(cleanedPhone);

            if (!isValid.IsSuccess)
                return DomainResult<Phone>.Failure(isValid.Error);

            return DomainResult<Phone>.Success(new Phone(code, cleanedPhone));
        }
        #endregion

        #region Restoration
        public static Phone Restore(CountryCode code, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw DomainDataInconsistencyException.EmptyOnRestore<Phone>(nameof(value));
            }

            return new Phone(code, value);
        }
        #endregion

        #region Behavior
        private static string CleanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            return new string(phoneNumber.Where(char.IsDigit).ToArray());
        }

        public static DomainResult IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return DomainResult.Failure(PhoneError.EmptyPhone);

            if (phoneNumber.Length < MIN_LENGTH_PHONE_NUMBER)
                return DomainResult.Failure(PhoneError.TooShort);

            if (phoneNumber.Length > MAX_LENGTH_PHONE_NUMBER)
                return DomainResult.Failure(PhoneError.TooLong);

            return DomainResult.Success();
        }
        #endregion

        public string GetFullNumber()
        {
            return $"{CountryCode}{Value}";
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return CountryCode;
            yield return Value;
        }
    }
}
