using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Application.Common.Errors.Users
{
    public static class UserErrors
    {
        public static Error UserAlreadyExists<TValue>(string loginName)
            => new(ApplicationErrorCodes.UsersError.UserAlreadyExists,
                   $"User with login {loginName} already exists. Owner: {typeof(TValue).Name}!",
                   ErrorType.Conflict
            );

        public static Error NotFound<TValue>(string login)
            => new(ApplicationErrorCodes.UsersError.UserNotFound,
                   $"User {login} not found. Owner: {typeof(TValue).Name}!",
                   ErrorType.NotFound
            );

        public static Error UserIsBlocked<TValue>(TimeSpan remainingTime)
            => new(ApplicationErrorCodes.UsersError.UserIsBlocked,
                   $"User is blocked. Block expires in: {remainingTime}. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error UserIsPermanentlyDeleted<TValue>()
            => new(ApplicationErrorCodes.UsersError.UserIsPermanentlyDeleted,
                   $"User permanently deleted. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error UserIsDeleted<TValue>(DateTimeOffset restoreUntil)
            => new(ApplicationErrorCodes.UsersError.UserIsDeleted,
                   $"User has been deleted. Available for restoration up to {restoreUntil}. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error InvalidPassword<TValue>(string login)
            => new(ApplicationErrorCodes.UsersError.UserInvalidPassword,
                   $"User {login} invalid password. Owner: {typeof(TValue).Name}!",
                   ErrorType.InvalidFormat
            );
    }
}
