using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Common.Errors.Users
{
    public static class PasswordErrors
    {
        public static Error EmptyPlainPassword<TValue>()
            => new(DomainErrorCodes.Password.EmptyPlainPassword,
                   $"The password is empty or missing. Owner: {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );

        public static Error MissingLowercaseLetterPlainPassword<TValue>()
            => new(DomainErrorCodes.Password.MissingLowercaseLetterPlainPassword,
                   $"Password must contain at least one lowercase letter. Owner: {typeof(TValue).Name}!",
                   ErrorType.Validation
            );

        public static Error MissingNumberPlainPassword<TValue>()
            => new(DomainErrorCodes.Password.NoContainNumber,
                   $"Password must contain at least one digit. Owner: {typeof(TValue).Name}!",
                   ErrorType.Validation
            );

        public static Error MissingUppercaseLetterPlainPassword<TValue>()
            => new(DomainErrorCodes.Password.MissingUppercaseLetterPlainPassword,
                   $"Password must contain at least one uppercase letter. Owner: {typeof(TValue).Name}!",
                   ErrorType.Validation
            );

        public static Error TooLongPassword<TValue>()
            => new(DomainErrorCodes.Password.TooLongPlainPassword,
                   $"The password is long. Owner: {typeof(TValue).Name}!",
                   ErrorType.Validation
            );

        public static Error TooShortPassword<TValue>()
            => new(DomainErrorCodes.Password.TooShortPlainPassword,
                   $"The password is short. Owner: {typeof(TValue).Name}!",
                   ErrorType.Validation
            );

        public static Error EmptyHashPassword<TValue>()
            => new(DomainErrorCodes.Password.EmptyHashPassword,
                   $"The password hash is empty or missing. Owner: {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );
    }
}
