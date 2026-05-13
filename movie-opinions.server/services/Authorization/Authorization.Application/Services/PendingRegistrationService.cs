using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Services;
using Authorization.Application.Interfaces.Time;
using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects;
using Authorization.Domain.ValueObjects.Login;

namespace Authorization.Application.Services
{
    public class PendingRegistrationService : IPendingRegistrationService
    {
        private readonly IUserPendingRegistrationRepository _userPendingRegistrationRepository;
        private readonly ISystemClock _systemClock;

        public PendingRegistrationService(
            IUserPendingRegistrationRepository userPendingRegistrationRepository,
            ISystemClock systemClock)
        {
            _userPendingRegistrationRepository = userPendingRegistrationRepository;
            _systemClock = systemClock;
        }

        public async Task<UserPendingRegistration> CreateOrRefreshAsync(Login login, Password password, CancellationToken cancellationToken)
        {
            var existing = await _userPendingRegistrationRepository.GetByLoginAsync(login, cancellationToken);

            if(existing != null)
            {
                existing.Refresh(password, _systemClock.UtcNow);

                await _userPendingRegistrationRepository.UpdateAsync(existing, cancellationToken);

                return existing;
            }

            var userRegistration = UserPendingRegistration.Create(login, password);

            var registration = await _userPendingRegistrationRepository.CreateAsync(userRegistration, cancellationToken);

            return registration;
        }
    }
}
