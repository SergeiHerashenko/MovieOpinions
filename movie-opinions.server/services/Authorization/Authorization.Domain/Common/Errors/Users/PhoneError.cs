namespace Authorization.Domain.Common.Errors.Users
{
    public static class PhoneError
    {
        public static Error EmptyPhone
            => new(ErrorCodes.PhoneError.EmptyPhone, "Phone cannot be empty!", ErrorType.EmptyValue);

        public static Error EmptyCode
            => new(ErrorCodes.PhoneError.EmptyCountryCode, "Country code cannot be empty!", ErrorType.EmptyValue);

        public static Error PhoneInvalidFormat
            => new(ErrorCodes.PhoneError.InvalidFormat, "Invalid phone format!", ErrorType.Validation);

        public static Error CountryCodeInvalidFormat
            => new(ErrorCodes.PhoneError.InvalidFormat, "Invalid country code format!", ErrorType.Validation);

        public static Error TooLong
            => new(ErrorCodes.PhoneError.TooLong, "Phone is too long!", ErrorType.Validation);

        public static Error TooShort
            => new(ErrorCodes.PhoneError.TooShort, "Phone is too short!", ErrorType.Validation);
    }
}
