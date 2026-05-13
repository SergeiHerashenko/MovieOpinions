namespace Authorization.Application.Exceptions
{
    public sealed class UnknowOperationException : BaseApplicationException
    {
        public UnknowOperationException(string errorCode, string message) 
            : base(errorCode, message) { }

        public UnknowOperationException(string errorCode, string message, Exception inner) 
            : base(errorCode, message, inner) { }
    }
}
