using Authorization.Application.Interfaces.Persistence;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersRestriction;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserRestrictionSessionRepository : RepositoryBase, IUserRestrictionSessionRepository
    {
        public AdoUserRestrictionSessionRepository(
            ILogger<AdoUserRestrictionSessionRepository> logger, 
            IDbConnectionProvider dbConnectionProvider) 
            : base(logger, dbConnectionProvider)
        {
        }

        public Task<UserRestrictionSession> CreateAsync(UserRestrictionSession entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestrictionSession> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestrictionSession?> GetSessionRestrictionByLoginAsync(Login login)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestrictionSession> UpdateAsync(UserRestrictionSession entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
