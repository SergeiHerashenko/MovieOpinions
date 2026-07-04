using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersRefreshToken;

namespace Authorization.Application.Interfaces.Persistence
{
    public interface IUserRefreshTokenRepository : IBaseRepository<UserRefreshToken>
    {
        Task<UserRefreshToken> GetTokenByIdUser(UserId userId, CancellationToken cancellationToken = default);
    }
}
