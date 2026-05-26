using Authorization.Domain.Common.Errors;

namespace Authorization.Domain.Common.Exceptions
{
    public sealed class DomainInvalidOperationException : BaseDomainException
    {
        private DomainInvalidOperationException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        public static DomainInvalidOperationException ValueAccessOnFailure(
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
