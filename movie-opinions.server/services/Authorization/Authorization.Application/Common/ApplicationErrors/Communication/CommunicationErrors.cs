using Authorization.Domain.Common.Errors;

namespace Authorization.Application.Common.ApplicationErrors.Communication
{
    public static class CommunicationErrors
    {
        public static Error NotSent<TValue>()
            => new(ApplicationErrorCodes.CommunicationErrorCode.NotSent,
                   $"Error sending. Owner: {typeof(TValue).Name}",
                   ErrorType.InvalidOperation
            );
    }
}
