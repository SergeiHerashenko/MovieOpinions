using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.Dapper
{
    public class DapperUserTokenRepository : RepositoryBase, IUserRefreshTokenRepository
    {
        public DapperUserTokenRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<DapperUserTokenRepository> logger)
                : base(logger, dbConnectionProvider) { }

        public Task<UserRefreshToken> CreateAsync(UserRefreshToken entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserRefreshToken> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserRefreshToken>> GetAllTokensUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserRefreshToken?> GetUserTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserRefreshToken> UpdateAsync(UserRefreshToken entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
