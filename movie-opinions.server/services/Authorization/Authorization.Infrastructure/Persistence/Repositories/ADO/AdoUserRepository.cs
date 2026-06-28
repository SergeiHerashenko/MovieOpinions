using Authorization.Application.Interfaces.Persistence;
using Authorization.Domain.Users;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserRepository : RepositoryBase, IUserRepository
    {
        public AdoUserRepository(
            IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserRepository> logger)
            : base(logger, dbConnectionProvider) { }

        public async Task<User> CreateAsync(User entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<User> UpdateAsync(User entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<User> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExistsByLoginAsync(Login login, CancellationToken cancellationToken = default)
        {
            return false;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetUserByLoginAsync(Login login, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
