using Authorization.Application.Common.ApplicationErrors;
using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Application.Common.Exceptions
{
    public sealed class ApplicationInvalidOperationException : BaseException
    {
        private ApplicationInvalidOperationException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object> context,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        #region ValueAccessOnFailure
        public static ApplicationInvalidOperationException ValueAccessOnFailure<TEntity>(
            string valueName,
            OperationType operationType = OperationType.Reading,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = new Dictionary<string, object>()
            {
                ["Layer"] = "Application",
                ["Entity"] = typeof(TEntity).Name,
                ["ValueName"] = valueName,
            };

            var errorMessage = message ?? BuildValueAccessOnFailureMessage<TEntity>(valueName);

            return new(
                ApplicationErrorCodes.InvalidOperationErrorCode.InvalidOperation,
                ErrorType.InvalidOperation,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildValueAccessOnFailureMessage<TEntity>(string valueName)
        {
            return $"Unable to access the value '{valueName}'. Entity '{typeof(TEntity).Name}'!"; ;
        }
        #endregion
    }
}
