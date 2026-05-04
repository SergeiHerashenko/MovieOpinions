namespace Authorization.Domain.Exceptions
{
    public class SecurityDomainException : BaseDomainException
    {
        public SecurityDomainException(string errorCode, string message) 
            : base(errorCode, message) { }
    }
}
