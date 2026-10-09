using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Errors
{
    /// <summary>
    /// Містить фабричні методи доменних помилок,
    /// пов’язаних із повною електронною адресою.
    ///
    /// (Provides domain-error factories related
    /// to a complete email address.)
    /// </summary>
    public static class EmailErrors
    {
        public static Error EmptyEmail<TType>()
            => new(
                DomainErrorCodes.Email.EmptyEmail,
                $"Email address is empty or missing. Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error InvalidFormatEmail<TType>()
            => new(
                DomainErrorCodes.Email.InvalidFormatEmail,
                $"Email address has an invalid structure. It must contain exactly one " +
                $"'@' separator, non-empty local and domain parts, and no whitespace. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error TooLongEmail<TType>(
            int actualLength,
            int maximumLength)
            => new(
                DomainErrorCodes.Email.TooLongEmail,
                $"Email address length '{actualLength}' exceeds the maximum " +
                $"allowed length of '{maximumLength}'. Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );
    }
}
