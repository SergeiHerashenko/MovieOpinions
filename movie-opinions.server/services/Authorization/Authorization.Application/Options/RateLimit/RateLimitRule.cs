namespace Authorization.Application.Options.RateLimit
{
    public sealed class RateLimitRule
    {
        public int MaxAttempts { get; set; }

        public TimeSpan Window { get; set; }
    }
}
