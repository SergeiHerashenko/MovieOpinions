namespace Authorization.Domain.Exceptions
{
    public abstract class BaseDomainException : Exception
    {
        public string ErrorCode { get; }

        protected BaseDomainException(string code, string? technicalMessage = null)
            : base(technicalMessage)
        {
            ErrorCode = code;
        }

        protected BaseDomainException(string code, string? technicalMessage, Exception innerException)
            : base(technicalMessage, innerException)
        {
            ErrorCode = code;
        }
    }
}
