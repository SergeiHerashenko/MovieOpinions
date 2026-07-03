using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions;
using Authorization.Infrastructure.Errors;

namespace Authorization.Infrastructure.Exceptions
{
    public sealed class ReturningNoDataException : BaseException
    {
        private ReturningNoDataException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context ?? new Dictionary<string, object>(), innerException) { }

        public static ReturningNoDataException NoDataReceived(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                InfrastructureErrorCodes.DbError.DataNoReceived,
                ErrorType.InvalidOperation,
                message,
                context,
                innerException
            );
        }
    }
}
