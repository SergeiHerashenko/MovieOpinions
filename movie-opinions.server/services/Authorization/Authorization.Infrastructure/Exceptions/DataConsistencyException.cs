namespace Authorization.Infrastructure.Exceptions
{
    public sealed class DataConsistencyException : BaseInfrastructureException
    {
        private const string DefaultErrorCode = "DATA_CONSISTENCY_ERROR";

        public DataConsistencyException(string message)
            : base(DefaultErrorCode, message) { }

        public DataConsistencyException(string errorCode, string message, Exception inner)
            : base(errorCode, message, inner) { } 
    }
}
