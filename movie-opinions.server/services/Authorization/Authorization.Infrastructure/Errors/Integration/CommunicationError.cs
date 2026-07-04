using Authorization.Domain.Common.Errors;

namespace Authorization.Infrastructure.Errors.Integration
{
    public static class CommunicationError
    {
        public static Error SendError<TValue>(string message)
            => new(InfrastructureErrorCodes.IntegrationError.SendingError,
                $"{message}. Owner: {typeof(TValue).Name}!",
                ErrorType.Forbidden
            );
    }
}
