namespace Authorization.Infrastructure.Exceptions
{
    public abstract class BaseInfrastructureException : Exception
    {
        public string ErrorCode { get; }

        protected BaseInfrastructureException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = string.IsNullOrWhiteSpace(errorCode)
                ? "UNKNOWN_ERROR"
                : errorCode;
        }

        protected BaseInfrastructureException(string errorCode, string message, Exception inner)
            : base(message, inner)
        {
            ErrorCode = string.IsNullOrWhiteSpace(errorCode)
                ? "UNKNOWN_ERROR"
                : errorCode;
        }
    }
}
