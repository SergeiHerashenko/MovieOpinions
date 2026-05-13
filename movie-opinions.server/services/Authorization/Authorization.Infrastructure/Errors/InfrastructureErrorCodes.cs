namespace Authorization.Infrastructure.Errors
{
    public static class InfrastructureErrorCodes
    {
        public static class RateLimitError
        {
            public const string TooManyAttempts = "RATE_LIMIT_TOO_MANY_ATTEMPTS";

            public const string NotFoundConfiguration = "CONFIGURATION_NOT_FOUND";
        }
    }
}
