using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Errors
{
    /// <summary>
    /// Надає очікувані доменні помилки,
    /// пов’язані з перевіркою пароля у відкритому вигляді.
    ///
    /// Повідомлення не повинні містити саме значення пароля.
    ///
    /// (Provides expected domain errors associated
    /// with plaintext-password validation.
    ///
    /// Messages must never contain the password value itself.)
    /// </summary>
    public static class PlainPasswordErrors
    {
        public static Error EmptyPlainPassword<TType>()
            => new(
                DomainErrorCodes.PlainPassword.EmptyPlainPassword,
                $"Plain password is empty, missing, or contains only whitespace. Owner: '{typeof(TType).Name}'!",
                ErrorType.Validation
            );

        public static Error MissingLowercaseLetterPlainPassword<TType>()
            => new(
                DomainErrorCodes.PlainPassword.MissingLowercaseLetterPlainPassword,
                $"Plain password must contain at least one lowercase letter. Owner: '{typeof(TType).Name}'!",
                ErrorType.Validation
            );

        public static Error MissingUppercaseLetterPlainPassword<TType>()
            => new(
                DomainErrorCodes.PlainPassword.MissingUppercaseLetterPlainPassword,
                $"Plain password must contain at least one uppercase letter. Owner: '{typeof(TType).Name}'!",
                ErrorType.Validation
            );

        public static Error MissingDigitPlainPassword<TType>()
            => new(
                DomainErrorCodes.PlainPassword.MissingNumberPlainPassword,
                $"Plain password must contain at least one digit. Owner: '{typeof(TType).Name}'!",
                ErrorType.Validation
            );

        public static Error TooLongPlainPassword<TType>(
            int actualLength,
            int maximumLength)
            => new(
                DomainErrorCodes.PlainPassword.TooLongPlainPassword,
                $"Plain password length exceeds the allowed maximum. Actual length: '{actualLength}'. " +
                $"Maximum length: '{maximumLength}'. Owner: '{typeof(TType).Name}'!",
                ErrorType.Validation
            );

        public static Error TooShortPlainPassword<TType>(
            int actualLength,
            int minimumLength)
            => new(
                DomainErrorCodes.PlainPassword.TooShortPlainPassword,
                $"Plain password length is below the required minimum. Actual length: '{actualLength}'. " +
                $"Minimum length: '{minimumLength}'. Owner: '{typeof(TType).Name}'!",
                ErrorType.Validation
            );
    }
}
