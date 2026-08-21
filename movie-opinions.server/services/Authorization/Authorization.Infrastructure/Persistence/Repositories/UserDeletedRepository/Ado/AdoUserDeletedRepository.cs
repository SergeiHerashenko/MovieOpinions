using Authorization.Application.Abstractions.Persistence;
using Authorization.Domain.Users.Entities.UsersDeletion;

namespace Authorization.Infrastructure.Persistence.Repositories.UserDeletedRepository.Ado
{
    internal class AdoUserDeletedRepository : IUserDeletedRepository
    {
        private readonly AdoUserDeletedCommandRepository _adoUserDeletedCommandRepository;
        private readonly AdoUserDeletedQueryRepository _adoUserDeletedQueryRepository;

        public AdoUserDeletedRepository(
            AdoUserDeletedCommandRepository adoUserDeletedCommandRepository,
            AdoUserDeletedQueryRepository adoUserDeletedQueryRepository)
        {
            _adoUserDeletedCommandRepository = adoUserDeletedCommandRepository;
            _adoUserDeletedQueryRepository = adoUserDeletedQueryRepository;
        }

        public Task CreateDeletedUserAsync(UserDeletion entity, CancellationToken cancellationToken = default)
            => _adoUserDeletedCommandRepository.CreateDeletedUserAsync(entity, cancellationToken);

        public Task UpdateDeletedUserAsync(UserDeletion entity, CancellationToken cancellationToken = default)
            => _adoUserDeletedCommandRepository.UpdateDeletedUserAsync(entity, cancellationToken);
    }
}
