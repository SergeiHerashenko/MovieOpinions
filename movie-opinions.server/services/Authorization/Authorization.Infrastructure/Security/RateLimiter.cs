using Authorization.Application.Common.Enums;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Options.RateLimit;
using Authorization.Domain.Results;
using Authorization.Domain.UsersRefreshToken.ValueObjects;
using Authorization.Infrastructure.Errors.RateLimiter;
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

        public async Task<Result> EnsureAllowedAsync(RateLimitAction action, IpAddress ip, string identifier, CancellationToken cancellationToken = default)
        {
            if(!_options.Rules.TryGetValue(action, out var config))
            {
                throw InfrastructureConfigurationException.ValueNotFound(
                    $"Failed to process rate limit. Action '{action}' is not configured in the system component!",
                    new Dictionary<string, object>
                    {
                        ["layer"] = "Infrastructure",
                        ["action"] = action.ToString()
                    }
                );
            }

            var key = BuildKey(action, ip, identifier);

            var currentBytes = await _cache.GetAsync(key, cancellationToken);

            if(currentBytes is not  null)
            {
                int attempts = int.Parse(Encoding.UTF8.GetString(currentBytes));

                if (attempts >= config.MaxAttempts)
                {
                    return Result.Failure(RateLimiterErrors.LimitExceeded<RateLimiter>(action));
                }

                attempts++;
                var newBytes = Encoding.UTF8.GetBytes(attempts.ToString());

                await _cache.SetAsync(key, newBytes, cancellationToken);
            }
            else
            {
                var initialBytes = Encoding.UTF8.GetBytes("1");
                await _cache.SetAsync(key, initialBytes, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = config.Window
                }, cancellationToken);
            }

            return Result.Success();
        }

        private static string BuildKey(RateLimitAction action, IpAddress ip, string identifier)
        {
            return $"rl:{action}:{ip.Value}:{identifier.ToLowerInvariant()}";
        }
    }
}
