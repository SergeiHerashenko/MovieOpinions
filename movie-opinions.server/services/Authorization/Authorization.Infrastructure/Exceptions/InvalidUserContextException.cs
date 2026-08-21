using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions;
using Authorization.Infrastructure.Errors;

namespace Authorization.Infrastructure.Exceptions
{
    public sealed class InvalidUserContextException : BaseException
    {
        private InvalidUserContextException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context ?? new Dictionary<string, object>(), innerException) { }

        public static InvalidUserContextException Unauthorized(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new InvalidUserContextException(
                InfrastructureErrorCodes.UsereContextError.InvalidUserContext,
                ErrorType.InvalidFormat,
                message,
                context,
                innerException
            );
        }
    }
}
