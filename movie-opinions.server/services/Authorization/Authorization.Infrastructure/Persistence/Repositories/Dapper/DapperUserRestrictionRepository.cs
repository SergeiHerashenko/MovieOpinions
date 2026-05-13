using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.Dapper
{
    public class DapperUserRestrictionRepository : RepositoryBase, IUserRestrictionRepository
    {
        public DapperUserRestrictionRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<DapperUserRestrictionRepository> logger)
                : base(logger, dbConnectionProvider) { }

        public Task<UserRestriction> CreateAsync(UserRestriction entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction?> GetActiveBanByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserRestriction>> GetAllBansByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction?> GetBanByIdAsync(Guid banId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserRestriction>> GetBansByAdminNicknameAsync(string adminNickname, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction> UpdateAsync(UserRestriction entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
