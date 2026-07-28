using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Common.Errors.Users
{
    public static class LoginErrors
    {
        public static Error EmptyLogin<TValue>()
            => new(DomainErrorCodes.Login.Empty,
                   $"The login for '{typeof(TValue).Name}' is empty or missing!",
                   ErrorType.EmptyValue
            );

        public static Error LoginIsNotConfirm<TValue>()
            => new(DomainErrorCodes.Login.LoginIsNotConfirm,
                   $"Login is not confirmed. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );
    }
}
