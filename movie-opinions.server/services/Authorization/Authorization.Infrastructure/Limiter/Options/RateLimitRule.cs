namespace Authorization.Infrastructure.Limiter.Options
{
    public sealed class RateLimitRule
    {
        public int MaxAttempts { get; set; }

        public TimeSpan Window { get; set; }
    }
}
