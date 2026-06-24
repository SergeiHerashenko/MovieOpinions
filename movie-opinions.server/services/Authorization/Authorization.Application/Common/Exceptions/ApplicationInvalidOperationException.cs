using Authorization.Domain.Common.Errors;

namespace Authorization.Application.Common.Exceptions
{
    public sealed class ApplicationInvalidOperationException : BaseApplicationException
    {
        private ApplicationInvalidOperationException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        public static ApplicationInvalidOperationException ValueAccessOnFailure(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                ErrorCodes.ResultError.ValueAccessOnFailure,
                ErrorType.InvalidOperation,
                message,
                context,
                innerException
            );
        }
    }
}
