using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Errors
{
    /// <summary>
    /// Містить фабричні методи доменних помилок, пов’язаних
    /// із локальною частиною електронної адреси.
    ///
    /// (Provides domain-error factories related
    /// to the local part of an email address.)
    /// </summary>
    public static class EmailLocalErrors
    {
        public static Error EmptyEmailLocal<TType>()
            => new(
                DomainErrorCodes.EmailLocal.EmptyEmailLocal,
                $"Email local part is empty or missing. Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error InvalidFormatEmailLocal<TType>()
            => new(
                DomainErrorCodes.EmailLocal.InvalidFormatEmailLocal,
                $"Email local part has an invalid format. Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error TooLongEmailLocal<TType>(
            int actualLength,
            int maximumLength)
            => new(
                DomainErrorCodes.EmailLocal.TooLongEmailLocal,
                $"Email local part length '{actualLength}' exceeds the maximum " +
                $"allowed length of '{maximumLength}'. Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error TooShortEmailLocal<TType>(
            int actualLength,
            int minimumLength)
            => new(
                DomainErrorCodes.EmailLocal.TooShortEmailLocal,
                $"Email local part length '{actualLength}' is shorter than the minimum " +
                $"allowed length of '{minimumLength}'. Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );
    }
}
