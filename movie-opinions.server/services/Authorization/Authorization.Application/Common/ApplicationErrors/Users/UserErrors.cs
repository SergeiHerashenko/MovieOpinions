using Authorization.Domain.Common.Errors;

namespace Authorization.Application.Common.ApplicationErrors.Users
{
    public static class UserErrors
    {
        public static Error Exists<TValue>(string loginName)
            => new(ApplicationErrorCodes.UserErrorCode.AlreadyExists,
                   $"User with login {loginName} already exists. Owner: {typeof(TValue).Name}!",
                   ErrorType.Conflict
            );
    }
}
