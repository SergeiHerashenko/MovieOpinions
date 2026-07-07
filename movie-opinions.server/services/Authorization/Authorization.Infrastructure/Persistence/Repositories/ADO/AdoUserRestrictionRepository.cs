using Authorization.Application.Interfaces.Persistence;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersRestriction;
using Authorization.Domain.UsersRestriction.ValueObjects;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserRestrictionRepository : RepositoryBase, IUserRestrictionRepository
    {
        public AdoUserRestrictionRepository(
            ILogger<AdoUserRestrictionRepository> logger, 
            IDbConnectionProvider dbConnectionProvider) 
            : base(logger, dbConnectionProvider)
        {
        }

        public Task<UserRestriction> CreateAsync(UserRestriction entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction?> GetRestrictionByIdAsync(UserRestrictionId userRestrictionId)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction?> GetRestrictionByLoginAsync(Login login)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserRestriction>?> GetRestrictionsAsync(IEnumerable<UserRestrictionId> userRestrictionIds)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction> UpdateAsync(UserRestriction entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
