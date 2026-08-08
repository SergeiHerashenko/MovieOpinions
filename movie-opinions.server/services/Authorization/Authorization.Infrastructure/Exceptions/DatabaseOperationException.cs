using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions;
using Authorization.Infrastructure.Errors;

namespace Authorization.Infrastructure.Exceptions
{
    public sealed class DatabaseOperationException : BaseException
    {
        private DatabaseOperationException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context ?? new Dictionary<string, object>(), innerException) { }

        public static DatabaseOperationException ConnectionStringNotFound(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                InfrastructureErrorCodes.DbError.ConnectionStringNotFound,
                ErrorType.InvalidOperation,
                message,
                context,
                innerException
            );
        }

        public static DatabaseOperationException DatabaseError(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                InfrastructureErrorCodes.DbError.DatabaseError,
                ErrorType.InvalidOperation,
                message,
                context,
                innerException
            );
        }

        public static DatabaseOperationException NoTransaction(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                InfrastructureErrorCodes.DbError.NoTransaction,
                ErrorType.InvalidOperation,
                message,
                context,
                innerException
            );
        }

        public static DatabaseOperationException NestedTransaction(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                InfrastructureErrorCodes.DbError.NestedTransaction,
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
