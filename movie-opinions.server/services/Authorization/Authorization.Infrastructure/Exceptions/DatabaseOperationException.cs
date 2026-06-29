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

        public static DatabaseOperationException NotFoundFile(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                InfrastructureErrorCodes.DbError.NotFound,
                ErrorType.InvalidOperation,
                message,
                context,
                innerException
            );
        }

        public static DatabaseOperationException ExceptionMigrate(
            string message,
            Exception innerException,
            IReadOnlyDictionary<string, object>? context = null)
        {
            return new(
                InfrastructureErrorCodes.DbError.MigrationError,
                ErrorType.InvalidOperation,
                message,
                context,
                innerException
            );
        }
    }
}
