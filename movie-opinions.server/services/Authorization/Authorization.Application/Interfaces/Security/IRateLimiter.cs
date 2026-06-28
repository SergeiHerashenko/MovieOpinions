using Authorization.Application.Common.Enums;
using Authorization.Application.Result;

namespace Authorization.Application.Interfaces.Security
{
    public interface IRateLimiter
    {
        Task<ApplicationResult> EnsureAllowedAsync(RateLimitAction action, string ip, string login, CancellationToken cancellationToken = default);
    }
}
