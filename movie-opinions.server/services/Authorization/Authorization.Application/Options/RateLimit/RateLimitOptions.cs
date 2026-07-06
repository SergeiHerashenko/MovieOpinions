using Authorization.Application.Common.Enums;

namespace Authorization.Application.Options.RateLimit
{
    public sealed class RateLimitOptions
    {
        public const string SectionName = "RateLimit";

        public Dictionary<RateLimitAction, RateLimitRule> Rules { get; set; } = new();
    }
}
