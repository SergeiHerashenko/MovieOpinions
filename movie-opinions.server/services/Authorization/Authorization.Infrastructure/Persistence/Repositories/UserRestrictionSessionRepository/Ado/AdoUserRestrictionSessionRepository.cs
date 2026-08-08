using Authorization.Application.Abstractions.Persistence;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRestrictionSessionRepository.Ado
{
    internal class AdoUserRestrictionSessionRepository : IUserRestrictionSessionRepository
    {
        private readonly AdoUserRestrictionSessionQueryRepository _adoUserRestrictionSessionQueryRepository;
        private readonly AdoUserRestrictionSessionCommandRepository _adoUserRestrictionSessionCommandRepository;

        public AdoUserRestrictionSessionRepository(
            AdoUserRestrictionSessionQueryRepository adoUserRestrictionSessionQueryRepository,
            AdoUserRestrictionSessionCommandRepository adoUserRestrictionSessionCommandRepository)
        {
            _adoUserRestrictionSessionQueryRepository = adoUserRestrictionSessionQueryRepository;
            _adoUserRestrictionSessionCommandRepository = adoUserRestrictionSessionCommandRepository;
        }

        public Task CreateRestrictionSessionAsync(UserRestrictionSession entity, CancellationToken cancellationToken = default)
            => _adoUserRestrictionSessionCommandRepository.CreateRestrictionSessionAsync(entity, cancellationToken);

        public Task UpdateRestrictionSessionAsync(UserRestrictionSession entity, CancellationToken cancellationToken = default)
            => _adoUserRestrictionSessionCommandRepository.UpdateRestrictionSessionAsync(entity, cancellationToken);

        public Task DeleteRestrictionSessionAsync(UserRestrictionSessionId userRestrictionSessionId, CancellationToken cancellationToken = default)
            => _adoUserRestrictionSessionCommandRepository.DeleteRestrictionSessionAsync(userRestrictionSessionId, cancellationToken);

        public Task<UserRestrictionSession?> GetRestrictionSessionByIdAsync(UserRestrictionSessionId userRestrictionSessionId, CancellationToken cancellationToken = default)
            => _adoUserRestrictionSessionQueryRepository.GetRestrictionSessionByIdAsync(userRestrictionSessionId, cancellationToken);

        public Task<UserRestrictionSession?> GetRestrictionSessionByUserIdAndTypeAsync(UserId userId, RestrictionType type, CancellationToken cancellationToken = default)
            => _adoUserRestrictionSessionQueryRepository.GetRestrictionSessionByUserIdAndTypeAsync(userId, type, cancellationToken);

        public Task<IReadOnlyList<UserRestrictionSession>> GetRestrictionsSessionsByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
            => _adoUserRestrictionSessionQueryRepository.GetRestrictionsSessionsByUserIdAsync(userId, cancellationToken);
    }
}
