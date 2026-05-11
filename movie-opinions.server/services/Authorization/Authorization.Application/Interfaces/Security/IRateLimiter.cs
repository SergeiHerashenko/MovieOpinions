using Authorization.Application.Common.Enum;

namespace Authorization.Application.Interfaces.Security
{
    public interface IRateLimiter
    {
        Task EnsureAllowedAsync(RateLimitAction action, string ip, string login, CancellationToken cancellationToken = default);
    }
}
