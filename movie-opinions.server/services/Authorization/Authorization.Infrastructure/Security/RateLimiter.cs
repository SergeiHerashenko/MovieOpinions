using Authorization.Application.Common.Enum;
using Authorization.Application.Interfaces.Security;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Security
{
    public sealed class RateLimiter : IRateLimiter
    {
        private readonly IDistributedCache _cache;

        public RateLimiter(
            IDistributedCache cache)
        {
            _cache = cache;
        }

        public Task EnsureAllowedAsync(RateLimitAction action, string ip, string login, CancellationToken cancellationToken = default)
        {
            //if (!_options.Rules.TryGetValue(action, out var config))
            //{
            //
            //}
        }
    }
}
