using Authorization.Domain.Common.Errors;

namespace Authorization.Application.Common.ApplicationErrors.Communication
{
    public static class CommunicationErrors
    {
        public static Error NotSent(string message)
            => new(ApplicationErrorCodes.CommunicationError.NotSent,
                   message,
                   ErrorType.InvalidOperation
            );
    }
}
