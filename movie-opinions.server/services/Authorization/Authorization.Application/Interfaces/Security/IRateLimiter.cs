using Authorization.Application.Common.Enums;
using Authorization.Domain.Results;
using Authorization.Domain.UsersRefreshToken.ValueObjects;

namespace Authorization.Application.Interfaces.Security
{
    public interface IRateLimiter
    {
        Task<Result> EnsureAllowedAsync(RateLimitAction action, IpAddress ip, string login, CancellationToken cancellationToken = default);
    }
}
