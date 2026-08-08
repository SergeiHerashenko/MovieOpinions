using Authorization.Domain.Users.Entities.UsersRefreshToken;

namespace Authorization.Application.Abstractions.Persistence
{
    public interface IUserRefreshTokenRepository
    {
        Task CreateRefreshTokenAsync(UserRefreshToken userRefreshToken, CancellationToken cancellationToken = default);

        Task UpdateRefreshTokenAsync(UserRefreshToken userRefreshToken, CancellationToken cancellationToken = default);
    }
}
