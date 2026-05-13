using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.Dapper
{
    public class DapperUserDeletionRepository : RepositoryBase, IUserDeletionRepository
    {
        public DapperUserDeletionRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<DapperUserDeletionRepository> logger)
                : base(logger, dbConnectionProvider) { }

        public Task<UserDeletion> CreateAsync(UserDeletion entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserDeletion> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserDeletion?> GetUserDeletionsByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserDeletion?> GetUserDeletionsByLoginAsync(string login, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserDeletion> UpdateAsync(UserDeletion entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
