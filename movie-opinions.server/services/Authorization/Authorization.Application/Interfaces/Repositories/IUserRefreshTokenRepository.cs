using Authorization.Domain.Entities;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserRefreshTokenRepository : IBaseRepository<UserRefreshToken>
    {
        Task<UserRefreshToken?> GetUserTokenAsync(string refreshToken, CancellationToken cancellationToken);

        Task<IEnumerable<UserRefreshToken>> GetAllTokensUserAsync(Guid userId, CancellationToken cancellationToken);
    }
}
