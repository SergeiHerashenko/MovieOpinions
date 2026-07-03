using Authorization.Domain.Common.Errors;

namespace Authorization.Infrastructure.Errors.Notification
{
    public static class NotificationError
    {
        public static Error SendError<TValue>(string message)
            => new(InfrastructureErrorCodes.IntegrationError.SendingError,
                $"{message}. Owner: {typeof(TValue).Name}!",
                ErrorType.Forbidden
            );
    }
}
