using Authorization.Application.Abstractions.Persistence;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRestrictionRepository.Ado
{
    internal class AdoUserRestrictionRepository : IUserRestrictionRepository
    {
        private readonly AdoUserRestrictionQueryRepository _adoUserRestrictionQueryRepository;
        private readonly AdoUserRestrictionCommandRepository _adoUserRestrictionCommandRepository;

        public AdoUserRestrictionRepository(
            AdoUserRestrictionQueryRepository adoUserRestrictionQueryRepository, 
            AdoUserRestrictionCommandRepository adoUserRestrictionCommandRepository)
        {
            _adoUserRestrictionQueryRepository = adoUserRestrictionQueryRepository;
            _adoUserRestrictionCommandRepository = adoUserRestrictionCommandRepository;
        }

        public Task CreateRestrictionAsync(UserRestriction entity, CancellationToken cancellationToken = default)
            => _adoUserRestrictionCommandRepository.CreateRestrictionAsync(entity, cancellationToken);

        public Task UpdateRestrictionAsync(UserRestriction entity, CancellationToken cancellationToken = default)
            => _adoUserRestrictionCommandRepository.UpdateRestrictionAsync(entity, cancellationToken);

        public Task<IReadOnlyList<UserRestriction>> GetRestrictionsByIdsAsync(IReadOnlyCollection<UserRestrictionId> userRestrictionIds, CancellationToken cancellationToken = default)
            => _adoUserRestrictionQueryRepository.GetRestrictionsByIdsAsync(userRestrictionIds, cancellationToken);
    }
}
