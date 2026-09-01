using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Application.Common.Errors.Users
{
    public static class RegistrationErrors
    {
        public static Error ConfirmPasswordRequired<TValue>()
            => new(ApplicationErrorCodes.RegistrationError.ConfirmPasswordRequired,
                   $"Confirm password is required. Owner: {typeof(TValue).Name}!",
                   ErrorType.Conflict
            );

        public static Error PasswordsDoNotMatch<TValue>()
            => new(ApplicationErrorCodes.RegistrationError.PasswordsDoNotMatch,
                   $"Passwords do not match. Owner: {typeof(TValue).Name}!",
                   ErrorType.Conflict
            );

        public static Error TermsMustBeAccepted<TValue>()
            => new(ApplicationErrorCodes.RegistrationError.TermsMustBeAccepted,
                   $"Terms of service must be accepted. Owner: {typeof(TValue).Name}!",
                   ErrorType.Conflict
            );
    }
}
