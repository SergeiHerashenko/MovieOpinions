namespace Authorization.Domain.Exceptions
{
    public sealed class DataInconsistencyDomainException : BaseDomainException
    {
        public DataInconsistencyDomainException(string errorCode, string message) 
            : base(errorCode, message) { }

        public DataInconsistencyDomainException(string errorCode, string message, Exception inner)
            : base(errorCode, message, inner) { }
    }
}
