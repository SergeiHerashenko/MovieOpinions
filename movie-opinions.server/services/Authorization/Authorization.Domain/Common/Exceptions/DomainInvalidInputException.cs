using Authorization.Domain.Common.Errors;

namespace Authorization.Domain.Common.Exceptions
{
    public sealed class DomainInvalidInputException : BaseDomainException
    {
        private DomainInvalidInputException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        public static DomainInvalidInputException InvariantViolation(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                ErrorCodes.ResultError.InvariantViolation,
                ErrorType.InvariantViolation,
                message,
                context,
                innerException
            );
        }
    }
}
