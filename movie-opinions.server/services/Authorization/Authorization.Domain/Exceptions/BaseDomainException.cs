namespace Authorization.Domain.Exceptions
{
    public abstract class BaseDomainException : Exception
    {
        public string ErrorCode { get; }

        protected BaseDomainException(string? code, string? message)
            : base(string.IsNullOrWhiteSpace(message) ? "Виникла помилка бізнес-логікию." : message)
        {
            ErrorCode = string.IsNullOrEmpty(code) ? "UNKNOWN_ERROR" : code;
        }

        protected BaseDomainException(string? code, string? message, Exception innerException)
            : base(string.IsNullOrWhiteSpace(message) ? "Виникла помилка бізнес-логікию." : message, innerException)
        {
            ErrorCode = string.IsNullOrEmpty(code) ? "UNKNOWN_ERROR" : code;
        }
    }
}
