namespace Authorization.Domain.Exceptions
{
    public sealed class ValidationDomainException : BaseDomainException
    {
        public ValidationDomainException(string errorCode, string message) 
            : base(errorCode, message) { }

        public ValidationDomainException(string errorCode, string message, Exception inner)
            : base(errorCode, message, inner) { }
    }
}
