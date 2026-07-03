using Authorization.Domain.Common.Errors;

namespace Authorization.Domain.Common.Exceptions
{
    public abstract class BaseException : Exception
    {
        public string ErrorCode { get; }

        public ErrorType ErrorType { get; }

        public IReadOnlyDictionary<string, object> Context { get; }

        protected BaseException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object> context,
            Exception? innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ErrorType = errorType;
            Context = context;
        }
    }
}
