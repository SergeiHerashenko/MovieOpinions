namespace Authorization.Domain.Exceptions
{
    public sealed class BusinessRuleViolationDomainException : BaseDomainException
    {
        public BusinessRuleViolationDomainException(string errorCode, string message) 
            : base(errorCode, message) { }

        public BusinessRuleViolationDomainException(string errorCode, string message, Exception inner)
            : base(errorCode, message, inner) { }
    }
}
