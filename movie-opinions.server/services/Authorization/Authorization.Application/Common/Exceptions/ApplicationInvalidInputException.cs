using Authorization.Domain.Common.Errors;

namespace Authorization.Application.Common.Exceptions
{
    public sealed class ApplicationInvalidInputException : BaseApplicationException
    {
        private ApplicationInvalidInputException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        public static ApplicationInvalidInputException InvariantViolation(
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
