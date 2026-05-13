namespace Authorization.Application.Exceptions
{
    public abstract class BaseApplicationException : Exception
    {
        public string ErrorCode { get; }

        protected BaseApplicationException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = string.IsNullOrWhiteSpace(errorCode)
                ? "UNKNOWN_ERROR"
                : errorCode;
        }

        protected BaseApplicationException(string errorCode, string message, Exception inner)
            : base(message, inner)
        {
            ErrorCode = string.IsNullOrWhiteSpace(errorCode)
                ? "UNKNOWN_ERROR"
                : errorCode;
        }
    }
}
