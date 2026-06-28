using Authorization.Domain.Common.Errors;

namespace Authorization.Infrastructure.Exceptions
{
    public abstract class BaseInfrastructureException : Exception
    {
        public string ErrorCode { get; }

        public ErrorType Type { get; }

        public IReadOnlyDictionary<string, object> Context { get; }

        protected BaseInfrastructureException(
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
