using Authorization.Domain.Common.Errors;

namespace Authorization.Application.Common.ApplicationErrors.Confirm
{
    public static class ConfirmErrors
    {
        public static Error InvalidOrExpiredToken<TValue>()
            => new(
                ApplicationErrorCodes.ConfirmErrorCode.InvalidToken,
                $"Registration request not found or outdated. Owner {typeof(TValue).Name}!",
                ErrorType.Forbidden
            );
    }
}
