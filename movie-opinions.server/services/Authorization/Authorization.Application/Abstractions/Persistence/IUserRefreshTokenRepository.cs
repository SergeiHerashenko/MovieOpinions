using Authorization.Domain.Users.AggregateChanges.Tokens;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;

namespace Authorization.Application.Abstractions.Persistence
{
    public interface IUserRefreshTokenRepository
    {
        Task CreateRefreshTokenAsync(UserRefreshToken userRefreshToken, CancellationToken cancellationToken = default);

        Task UpdateRefreshTokenAsync(UserRefreshToken userRefreshToken, CancellationToken cancellationToken = default);

        Task UpdateStatusRefreshTokenAsync(UserRefreshTokenUpdated userRefreshToken, CancellationToken cancellationToken = default);

        Task<UserRefreshToken?> GetRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

        Task<UserRefreshToken?> GetByTokenForUpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    }
}
