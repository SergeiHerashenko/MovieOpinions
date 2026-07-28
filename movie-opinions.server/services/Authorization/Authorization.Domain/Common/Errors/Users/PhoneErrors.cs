using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Common.Errors.Users
{
    public static class PhoneErrors
    {
        public static Error EmptyCountryCode<TValue>()
            => new(DomainErrorCodes.Phone.EmptyPhoneCountryCode,
                   $"Country code cannot be empty. Owner: {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );

        public static Error InvalidFormatCountryCode<TValue>(string invalidCountryCode)
            => new(DomainErrorCodes.Phone.InvalidFormatPhoneCountryCode,
                   $"The country code: '{invalidCountryCode}' has an invalid format. Owner: {typeof(TValue).Name}!",
                   ErrorType.InvalidFormat
            );

        public static Error TooLongCountryCode<TValue>(string countryCode)
            => new(DomainErrorCodes.Phone.TooLongPhoneCountryCode,
                   $"Country code '{countryCode}' is too long. Owner: {typeof(TValue).Name}",
                   ErrorType.OutOfRange
            );

        public static Error TooShortCountryCode<TValue>(string countryCode)
            => new(DomainErrorCodes.Phone.TooShortPhoneCountryCode,
                   $"Country code '{countryCode}' is too short. Owner {typeof(TValue).Name}",
                   ErrorType.OutOfRange
            );

        public static Error EmptyNationalNumber<TValue>()
            => new(DomainErrorCodes.Phone.EmptyPhoneNationalNumber,
                   $"National number cannot be empty. Owner: {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );

        public static Error InvalidFormatNationalNumber<TValue>(string invalidNationalNumber)
            => new(DomainErrorCodes.Phone.InvalidFormatPhoneNationalNumber,
                   $"The national number: '{invalidNationalNumber}' has an invalid format. Owner: {typeof(TValue).Name}!",
                   ErrorType.InvalidFormat
            );

        public static Error TooLongNationalNumber<TValue>(string nationalNumber)
            => new(DomainErrorCodes.Phone.TooLongPhoneNationalNumber,
                   $"National number '{nationalNumber}' is too long. Owner: {typeof(TValue).Name}",
                   ErrorType.OutOfRange
            );

        public static Error TooShortNationalNumber<TValue>(string nationalNumber)
            => new(DomainErrorCodes.Phone.TooShortPhoneNationalNumber,
                   $"National number '{nationalNumber}' is too short. Owner {typeof(TValue).Name}",
                   ErrorType.OutOfRange
            );

        public static Error NotAllowedPhone<TValue>(string phoneNumber)
            => new(DomainErrorCodes.Phone.NotAllowedPhone,
                   $"Phone number '{phoneNumber}' is not allowed for use!",
                   ErrorType.PolicyViolation
            );
    }
}
