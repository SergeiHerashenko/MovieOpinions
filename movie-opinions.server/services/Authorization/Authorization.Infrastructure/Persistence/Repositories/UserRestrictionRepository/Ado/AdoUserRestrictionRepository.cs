using Authorization.Application.Abstractions.Persistence;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRestrictionRepository.Ado
{
    internal class AdoUserRestrictionRepository : IUserRestrictionRepository
    {
        public Task CreateRestrictionAsync(UserRestriction entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRestrictionAsync(UserRestriction entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<UserRestriction>> GetRestrictionsByIdsAsync(IReadOnlyCollection<UserRestrictionId> userRestrictionIds, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
