namespace Authorization.Domain.Exceptions
{
    public class BusinessRuleViolationDomainException : BaseDomainException
    {
        public BusinessRuleViolationDomainException(string errorCode, string message) 
            : base(errorCode, message) { }
    }
}
