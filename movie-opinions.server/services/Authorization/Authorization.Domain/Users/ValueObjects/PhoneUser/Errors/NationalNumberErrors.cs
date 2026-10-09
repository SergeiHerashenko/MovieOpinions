using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Errors
{
    /// <summary>
    /// Містить фабрики очікуваних доменних помилок валідації
    /// національної частини телефонного номера.
    ///
    /// (Provides factories for expected domain validation errors
    /// related to the national part of a telephone number.)
    /// </summary>
    public static class NationalNumberErrors
    {
        public static Error EmptyNationalNumber<TType>()
            => new(
                DomainErrorCodes.NationalNumber.EmptyNationalNumber,
                $"National phone number is empty or missing. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error InvalidFormatNationalNumber<TType>()
            => new(
                DomainErrorCodes.NationalNumber.InvalidFormatNationalNumber,
                $"National phone number contains unsupported characters. " +
                $"Only ASCII digits, whitespace characters, hyphens, and parentheses are allowed. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error TooLongNationalNumber<TType>(
            int actualDigitCount,
            int maximumDigitCount)
            => new(
                DomainErrorCodes.NationalNumber.TooLongNationalNumber,
                $"National phone number contains too many digits. " +
                $"Actual digit count: {actualDigitCount}. " +
                $"Maximum allowed: {maximumDigitCount}. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error TooShortNationalNumber<TType>(
            int actualDigitCount,
            int minimumDigitCount)
            => new(
                DomainErrorCodes.NationalNumber.TooShortNationalNumber,
                $"National phone number contains too few digits. " +
                $"Actual digit count: {actualDigitCount}. " +
                $"Minimum required: {minimumDigitCount}. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );
    }
}
