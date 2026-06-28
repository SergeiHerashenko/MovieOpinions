using Authorization.Domain.Common.Errors;
using Authorization.Infrastructure.Errors;

namespace Authorization.Infrastructure.Exceptions
{
    public class DataConsistencyException : BaseInfrastructureException
    {
        private DataConsistencyException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

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
    }
}
