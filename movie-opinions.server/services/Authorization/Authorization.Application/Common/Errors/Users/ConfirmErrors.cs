using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Application.Common.Errors.Users
{
    public static class ConfirmErrors
    {
        public static Error InvalidOrExpiredToken<TValue>()
            => new(ApplicationErrorCodes.ConfirmError.InvalidToken,
                   $"Registration request not found or outdated. Owner {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );
    }
}
