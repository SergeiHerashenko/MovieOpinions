using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
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
        public static Result<Phone> Create(CountryCode code, string rawPhone)
        {
            if (code is null)
                return Result<Phone>.Failure(UserErrors.PhoneError.EmptyCountryCode<Phone>());

            if (string.IsNullOrWhiteSpace(rawPhone))
                return Result<Phone>.Failure(UserErrors.PhoneError.EmptyPhone<Phone>());

            var cleanedPhone = CleanPhoneNumber(rawPhone);

            if (string.IsNullOrEmpty(cleanedPhone))
                return Result<Phone>.Failure(UserErrors.PhoneError.PhoneInvalidFormat<Phone>());

            var isValid = IsValidPhoneNumber(cleanedPhone);

            if (!isValid.IsSuccess)
                return Result<Phone>.Failure(isValid.Errors);

            return Result<Phone>.Success(new Phone(code, cleanedPhone));
        }
        #endregion

        #region Restoration
        public static Phone Restore(CountryCode code, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw DomainDataInconsistencyException.Empty<Phone>(nameof(value));
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

        public static Result IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return Result.Failure(UserErrors.PhoneError.EmptyPhone<Phone>());

            if (phoneNumber.Length < MIN_LENGTH_PHONE_NUMBER)
                return Result.Failure(UserErrors.PhoneError.TooShort<Phone>());

            if (phoneNumber.Length > MAX_LENGTH_PHONE_NUMBER)
                return Result.Failure(UserErrors.PhoneError.TooLong<Phone>());

            return Result.Success();
        }
        #endregion

        public string GetFullNumber()
        {
            return $"{CountryCode.Value}{Value}";
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return CountryCode;
            yield return Value;
        }
    }
}
