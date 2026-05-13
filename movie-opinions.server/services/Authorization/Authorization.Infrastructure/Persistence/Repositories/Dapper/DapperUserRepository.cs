using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects.Login;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.Dapper
{
    public class DapperUserRepository : RepositoryBase, IUserRepository
    {
        public DapperUserRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<DapperUserRepository> logger)
                : base(logger, dbConnectionProvider) { }

        public Task<User> CreateAsync(User entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<User> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByLoginAsync(Login login, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByLoginAsync(Login login, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<User> UpdateAsync(User entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
