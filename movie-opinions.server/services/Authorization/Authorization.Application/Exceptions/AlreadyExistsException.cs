namespace Authorization.Application.Exceptions
{
    public sealed class AlreadyExistsException : BaseApplicationException
    {
        public AlreadyExistsException(string errorCode, string message) 
            : base(errorCode, message) { }
        
        public AlreadyExistsException(string errorCode, string message, Exception inner) 
            : base(errorCode, message, inner) { }
    }
}
