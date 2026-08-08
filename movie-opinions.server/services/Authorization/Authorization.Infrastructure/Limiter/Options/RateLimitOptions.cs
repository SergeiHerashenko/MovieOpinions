using Authorization.Application.Common.Enums;

namespace Authorization.Infrastructure.Limiter.Options
{
    public sealed class RateLimitOptions
    {
        public const string SectionName = "RateLimit";

        public Dictionary<RateLimitAction, RateLimitRule> Rules { get; set; } = new();
    }
}
