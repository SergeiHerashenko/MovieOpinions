using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Infrastructure.Errors.Communication
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
