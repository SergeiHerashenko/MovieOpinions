using Authorization.Application.Common.Enums;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;

namespace Authorization.Application.Abstractions.RateLimiter
{
    public interface IRateLimiter
    {
        Task<Result> EnsureAllowedAsync(RateLimitAction action, IpAddress ip, string login, CancellationToken cancellationToken = default);
    }
}
