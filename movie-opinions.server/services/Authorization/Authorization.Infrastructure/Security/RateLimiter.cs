using Authorization.Application.Common.Enum;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Options.RateLimit;
using Authorization.Infrastructure.Errors;
using Authorization.Infrastructure.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text;

namespace Authorization.Infrastructure.Security
{
    public sealed class RateLimiter : IRateLimiter
    {
        private readonly IDistributedCache _cache;
        private readonly RateLimitOptions _options;

        public RateLimiter(
            IDistributedCache cache,
            IOptions<RateLimitOptions> options)
        {
            _cache = cache;
            _options = options.Value;
        }

        public async Task EnsureAllowedAsync(RateLimitAction action, string ip, string login, CancellationToken cancellationToken = default)
        {
            if (!_options.Rules.TryGetValue(action, out var config))
            {
                throw new MissingRateLimitConfigurationException(
                    InfrastructureErrorCodes.RateLimitError.NotFoundConfiguration,
                    $"Rate limit configuration for action '{action}' was not found!"
                );
            }

            var key = BuildKey(action, ip, login);

            var currentBytes = await _cache.GetAsync(key, cancellationToken);

            int attempts = currentBytes is null ? 0 : int.Parse(Encoding.UTF8.GetString(currentBytes));

            if(attempts >= config.MaxAttempts)
            {
                throw new RateLimitExceededException(
                    InfrastructureErrorCodes.RateLimitError.TooManyAttempts,
                    "Too many requests. Please try again later"
                );
            }

            attempts++;
            var newBytes = Encoding.UTF8.GetBytes(attempts.ToString());

            await _cache.SetAsync(key, newBytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = config.Window
            }, cancellationToken);
        }

        private static string BuildKey(RateLimitAction action, string ip, string login)
        {
            return $"rl:{action}:{ip}:{login.ToLowerInvariant()}";
        }
    }
}
