namespace Authorization.Domain.Exceptions
{
    public sealed class ValidationDomainException : BaseDomainException
    {
        public ValidationDomainException(string errorCode, string message) 
            : base(errorCode, message) { }
    }
}
