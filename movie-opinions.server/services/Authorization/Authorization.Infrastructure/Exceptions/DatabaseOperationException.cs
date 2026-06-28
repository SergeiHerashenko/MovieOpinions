using Authorization.Domain.Common.Errors;
using Authorization.Infrastructure.Errors;

namespace Authorization.Infrastructure.Exceptions
{
    public sealed class DatabaseOperationException : BaseInfrastructureException
    {
        private DatabaseOperationException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        public static DatabaseOperationException NoConnection(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                InfrastructureErrorCodes.DbError.NoConnection,
                ErrorType.InvalidOperation,
                message,
                context,
                innerException
            );
        }
    }
}
