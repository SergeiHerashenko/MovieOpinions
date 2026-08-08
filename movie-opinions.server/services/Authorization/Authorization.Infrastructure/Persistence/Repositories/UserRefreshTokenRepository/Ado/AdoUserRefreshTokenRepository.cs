using Authorization.Application.Abstractions.Persistence;
using Authorization.Domain.Users.Entities.UsersRefreshToken;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRefreshTokenRepository.Ado
{
    internal class AdoUserRefreshTokenRepository : IUserRefreshTokenRepository
    {
        private readonly AdoUserRefreshTokenQueryRepository _adoUserRefreshTokenQueryRepository;
        private readonly AdoUserRefreshTokenCommandRepository _adoUserRefreshTokenCommandRepository;

        public AdoUserRefreshTokenRepository(
            AdoUserRefreshTokenQueryRepository adoUserRefreshTokenQueryRepository,
            AdoUserRefreshTokenCommandRepository adoUserRefreshTokenCommandRepository)
        {
            _adoUserRefreshTokenQueryRepository = adoUserRefreshTokenQueryRepository;
            _adoUserRefreshTokenCommandRepository = adoUserRefreshTokenCommandRepository;
        }

        public Task CreateRefreshTokenAsync(UserRefreshToken userRefreshToken, CancellationToken cancellationToken = default)
            => _adoUserRefreshTokenCommandRepository.CreateRefreshTokenAsync(userRefreshToken, cancellationToken);

        public Task UpdateRefreshTokenAsync(UserRefreshToken userRefreshToken, CancellationToken cancellationToken = default)
            => _adoUserRefreshTokenCommandRepository.UpdateRefreshTokenAsync(userRefreshToken, cancellationToken);
    }
}
