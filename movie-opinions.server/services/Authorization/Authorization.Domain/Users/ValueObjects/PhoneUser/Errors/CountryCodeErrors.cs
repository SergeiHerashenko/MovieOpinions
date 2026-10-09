using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Errors
{
    /// <summary>
    /// Містить фабричні методи доменних помилок,
    /// пов’язаних із міжнародним телефонним кодом країни.
    ///
    /// (Provides domain-error factories related
    /// to an international phone country calling code.)
    /// </summary>
    public static class CountryCodeErrors
    {
        public static Error EmptyCountryCode<TType>()
            => new(
                DomainErrorCodes.CountryCode.EmptyCountryCode,
                $"Phone country calling code is empty or missing. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error InvalidFormatCountryCode<TType>()
            => new(
                DomainErrorCodes.CountryCode.InvalidFormatCountryCode,
                $"Phone country calling code has an invalid format. It must begin with '+' " +
                $"and contain only ASCII digits, with the first digit from '1' to '9'. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error TooLongCountryCode<TType>(
            int actualLength,
            int maximumLength)
            => new(
                DomainErrorCodes.CountryCode.TooLongCountryCode,
                $"Phone country calling code length '{actualLength}' exceeds the maximum " +
                $"allowed length of '{maximumLength}'. Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error TooShortCountryCode<TType>(
            int actualLength,
            int minimumLength)
            => new(
                DomainErrorCodes.CountryCode.TooShortCountryCode,
                $"Phone country calling code length '{actualLength}' is shorter than the minimum " +
                $"allowed length of '{minimumLength}'. Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );
    }
}
