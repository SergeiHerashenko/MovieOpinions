namespace Authorization.Infrastructure.Exceptions
{
    public sealed class MissingRateLimitConfigurationException : BaseInfrastructureException
    {
        public MissingRateLimitConfigurationException(string errorCode, string message) 
            : base(errorCode, message) { }

        public MissingRateLimitConfigurationException(string errorCode, string message, Exception inner)
            : base(errorCode, message, inner) { }
    }
}
