using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersRefreshToken;

namespace Authorization.Application.Interfaces.Persistence
{
    public interface IUserRefreshTokenRepository : IBaseRepository<UserRefreshToken>
    {
        Task<UserRefreshToken> GetTokenByIdUserAsync(UserId userId, CancellationToken cancellationToken = default);
    }
}
