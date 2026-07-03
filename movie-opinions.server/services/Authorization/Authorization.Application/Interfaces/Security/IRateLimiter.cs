using Authorization.Application.Common.Enums;
using Authorization.Domain.Results;

namespace Authorization.Application.Interfaces.Security
{
    public interface IRateLimiter
    {
        Task<Result> EnsureAllowedAsync(RateLimitAction action, string ip, string login, CancellationToken cancellationToken = default);
    }
}
