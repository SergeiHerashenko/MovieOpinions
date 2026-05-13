namespace Authorization.Infrastructure.Exceptions
{
    public sealed class RateLimitExceededException : BaseInfrastructureException
    {
        public RateLimitExceededException(string errorCode, string message) 
            : base(errorCode, message) { }

        public RateLimitExceededException(string errorCode, string message, Exception inner)
            : base(errorCode, message, inner) { }
    }
}
