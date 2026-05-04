namespace Authorization.Domain.Exceptions
{
    public class ValidationDomainException : BaseDomainException
    {
        public ValidationDomainException(string errorCode, string message) 
            : base(errorCode, message) { }
    }
}
