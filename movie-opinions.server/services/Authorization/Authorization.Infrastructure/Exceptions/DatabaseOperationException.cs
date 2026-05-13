namespace Authorization.Infrastructure.Exceptions
{
    public sealed class DatabaseOperationException : BaseInfrastructureException
    {
        public DatabaseOperationException(string code, string message)
            : base(code, message) { }

        public DatabaseOperationException(string errorCode, string message, Exception inner)
            : base(errorCode, message, inner) { }
    }
}
