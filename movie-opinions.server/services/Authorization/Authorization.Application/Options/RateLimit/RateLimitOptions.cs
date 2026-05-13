using Authorization.Application.Common.Enum;

namespace Authorization.Application.Options.RateLimit
{
    public sealed class RateLimitOptions
    {
        public Dictionary<RateLimitAction, RateLimitRule> Rules { get; init; } = new();
    }
}
