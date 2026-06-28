using Authorization.Application.Common.ApplicationErrors.Users;
using Authorization.Application.Common.Enums;
using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Events;
using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Result;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration;

namespace Authorization.Application.Features.Authentication.Registration
{
    public class RegistrationFlowCoordinator
    {
        private readonly IRateLimiter _rateLomiter;
        private readonly IUserContext _userContext;
        private readonly IHasher _hasher;
        private readonly IClock _clock;

        private readonly IUserPendingRegistrationRepository _userPendingRegistrationRepository;
        private readonly IUserRepository _userRepository;

        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public RegistrationFlowCoordinator(
            IRateLimiter rateLomiter, 
            IUserContext userContext, 
            IHasher hasher, 
            IClock clock, 
            IUserPendingRegistrationRepository userPendingRegistrationRepository, 
            IUserRepository userRepository,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _rateLomiter = rateLomiter;
            _userContext = userContext;
            _hasher = hasher;
            _clock = clock;
            _userPendingRegistrationRepository = userPendingRegistrationRepository;
            _userRepository = userRepository;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<ApplicationResult<UserPendingRegistration>> ProcessAsync(
            Login login,
            string rawPassword,
            CancellationToken cancellationToken)
        {
            var resultLimiter = await _rateLomiter.EnsureAllowedAsync(
                RateLimitAction.Registration,
                _userContext.GetIpAddress(),
                login.Value,
                cancellationToken
            );

            if (resultLimiter.IsFailure)
                return ApplicationResult<UserPendingRegistration>.Failure(resultLimiter.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var exists = await _userRepository.ExistsByLoginAsync(login, cancellationToken);

            if (exists)
                // TODO: Подумати над фінальним текстом повідомлення, якщо сервіс впав
                return ApplicationResult<UserPendingRegistration>.Failure(UserErrors.Exists(""));

            var password = Password.Create(_hasher.Hash(rawPassword));

            if (password.IsFailure)
                return ApplicationResult<UserPendingRegistration>.Failure(password.Error);

            cancellationToken.ThrowIfCancellationRequested();

            var existingRegistration = await _userPendingRegistrationRepository.GetByLoginAsync(login, cancellationToken);

            UserPendingRegistration registration;

            if (existingRegistration is not null)
            {
                existingRegistration.Refresh(password.Value, _clock.UtcNow);

                await _userPendingRegistrationRepository.UpdateAsync(existingRegistration, cancellationToken);

                registration = existingRegistration;
            }
            else
            {
                var createResult = UserPendingRegistration.Create(login, password.Value);

                if (!createResult.IsSuccess)
                    return ApplicationResult<UserPendingRegistration>.Failure(createResult.Error);

                registration = createResult.Value;

                await _userPendingRegistrationRepository.CreateAsync(registration, cancellationToken);
            }

            foreach (var domainEvent in registration.DomainEvents)
                await _domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);

            registration.ClearDomainEvents();

            return ApplicationResult<UserPendingRegistration>.Success(registration);
        }
    }
}
