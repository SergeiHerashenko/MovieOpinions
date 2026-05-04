namespace Authorization.Domain.Exceptions
{
    public class InvalidStateDomainException : BaseDomainException
    {
        public InvalidStateDomainException(string errorCode, string message) 
            : base(errorCode, message) { }
    }
}
