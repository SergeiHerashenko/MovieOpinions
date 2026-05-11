namespace Authorization.Domain.Exceptions
{
    public sealed class DataInconsistencyDomainException : BaseDomainException
    {
        public DataInconsistencyDomainException(string errorCode, string message) 
            : base(errorCode, message) { }
    }
}
