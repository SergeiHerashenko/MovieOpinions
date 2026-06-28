using Authorization.Domain.Common.Errors;

namespace Authorization.Application.Common.ApplicationErrors.Users
{
    public static class UserErrors
    {
        public static Error Exists(string message)
            => new(ApplicationErrorCodes.UserError.AlreadyExists,
                   message,
                   ErrorType.Conflict
            );
    }
}
