using Authorization.Application.Abstractions.Persistence;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;

namespace Authorization.Infrastructure.Persistence.Repositories.UserPendingRegistrationRepository.Ado
{
    internal class AdoUserPendingRegistrationRepository : IUserPendingRegistrationRepository
    {
        private readonly AdoUserPendingRegistrationQueryRepository _adoUserPendingRegistrationQueryRepository;
        private readonly AdoUserPendingRegistrationCommandRepository _adoUserPendingRegistrationCommandRepository;

        public AdoUserPendingRegistrationRepository(
            AdoUserPendingRegistrationQueryRepository adoUserPendingRegistrationQueryRepository, 
            AdoUserPendingRegistrationCommandRepository adoUserPendingRegistrationCommandRepository)
        {
            _adoUserPendingRegistrationQueryRepository = adoUserPendingRegistrationQueryRepository;
            _adoUserPendingRegistrationCommandRepository = adoUserPendingRegistrationCommandRepository;
        }

        public Task CreatePendingUserAsync(UserPendingRegistration entity, CancellationToken cancellationToken = default)
            => _adoUserPendingRegistrationCommandRepository.CreatePendingUserAsync(entity, cancellationToken);

        public Task UpdatePendingUserAsync(UserPendingRegistration entity, CancellationToken cancellationToken = default)
            => _adoUserPendingRegistrationCommandRepository.UpdatePendingUserAsync(entity, cancellationToken);

        public Task DeletePendingUserAsync(UserPendingRegistrationId userPendingRegistrationId, CancellationToken cancellationToken = default)
            => _adoUserPendingRegistrationCommandRepository.DeletePendingUserAsync(userPendingRegistrationId, cancellationToken);

        public Task<UserPendingRegistration?> GetPendingUserByLoginAsync(Login login, CancellationToken cancellationToken = default)
            => _adoUserPendingRegistrationQueryRepository.GetPendingUserByLoginAsync(login, cancellationToken);

        public Task<UserPendingRegistration?> GetPendingUserByTokenAsync(RegistrationFlowToken registrationFlowToken, CancellationToken cancellationToken = default)
            => _adoUserPendingRegistrationQueryRepository.GetPendingUserByTokenAsync(registrationFlowToken, cancellationToken);
    }
}
