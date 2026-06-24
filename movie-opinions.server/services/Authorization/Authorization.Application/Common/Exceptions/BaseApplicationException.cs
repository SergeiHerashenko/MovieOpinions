using Authorization.Domain.Common.Errors;

namespace Authorization.Application.Common.Exceptions
{
    public abstract class BaseApplicationException : Exception
    {
        public string ErrorCode { get; }

        public ErrorType Type { get; }

        public IReadOnlyDictionary<string, object> Context { get; }

        protected BaseApplicationException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Type = errorType;
            Context = context ?? new Dictionary<string, object>();
        }
    }
}
