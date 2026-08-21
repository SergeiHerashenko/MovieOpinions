using Authorization.Application.Abstractions.Persistence;
using Authorization.Domain.Users.Entities.UsersPendingAction;

namespace Authorization.Infrastructure.Persistence.Repositories.UserPendingActionRepository.Ado
{
    internal class AdoUserPendingActionRepository : IUserPendingActionRepository
    {
        private readonly AdoUserPendingActionQueryRepository _adoUserPendingActionQueryRepository;
        private readonly AdoUserPendingActionCommandRepository _adoUserPendingActionCommandRepository;

        public AdoUserPendingActionRepository(
            AdoUserPendingActionQueryRepository adoUserPendingActionQueryRepository,
            AdoUserPendingActionCommandRepository adoUserPendingActionCommandRepository)
        {
            _adoUserPendingActionQueryRepository = adoUserPendingActionQueryRepository;
            _adoUserPendingActionCommandRepository = adoUserPendingActionCommandRepository;
        }

        public Task CreateActionUserAsync(UserPendingAction entity, CancellationToken cancellationToken = default)
            => _adoUserPendingActionCommandRepository.CreateActionUserAsync(entity, cancellationToken);

        public Task UpdateActionUserAsync(UserPendingAction entity, CancellationToken cancellationToken = default)
            => _adoUserPendingActionCommandRepository.UpdateActionUserAsync(entity, cancellationToken);
    }
}
