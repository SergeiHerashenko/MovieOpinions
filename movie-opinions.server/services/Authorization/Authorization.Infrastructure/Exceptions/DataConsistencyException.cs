using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions;
using Authorization.Infrastructure.Errors;

namespace Authorization.Infrastructure.Exceptions
{
    public sealed class DataConsistencyException : BaseException
    {
        private DataConsistencyException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context ?? new Dictionary<string, object>(), innerException) { }

        public static DataConsistencyException UnknownType(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                InfrastructureErrorCodes.DbError.DataConsistency,
                ErrorType.Conflict,
                message,
                context,
                innerException
            );
        }

        public static DataConsistencyException InvalidData(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                InfrastructureErrorCodes.DbError.DataConsistency,
                ErrorType.InvalidFormat,
                message,
                context,
                innerException
            );
        }

        public static DataConsistencyException NotConsistentState(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                InfrastructureErrorCodes.DbError.NotConsistentState,
                ErrorType.InvariantViolation,
                message,
                context,
                innerException
            );
        }
    }
}
