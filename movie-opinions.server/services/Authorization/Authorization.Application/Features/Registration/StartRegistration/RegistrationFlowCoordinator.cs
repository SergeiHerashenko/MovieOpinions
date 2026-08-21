using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Events;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.Security.Hashers;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Errors.Users;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;
using Authorization.Domain.UsersPendingRegistration;

namespace Authorization.Application.Features.Registration.StartRegistration
{
    public class RegistrationFlowCoordinator
    {
        private readonly IRateLimiter _rateLimiter;
        private readonly IUserContext _userContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IClock _clock;

        private readonly IUserRepository _userRepository;
        private readonly IUserPendingRegistrationRepository _userPendingRegistrationRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public RegistrationFlowCoordinator(
            IRateLimiter rateLimiter,
            IUserContext userContext,
            IPasswordHasher passwordHasher,
            IClock clock,
            IUserRepository userRepository,
            IUserPendingRegistrationRepository userPendingRegistrationRepository,
            IUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _rateLimiter = rateLimiter;
            _userContext = userContext;
            _passwordHasher = passwordHasher;
            _clock = clock;
            _userRepository = userRepository;
            _userPendingRegistrationRepository = userPendingRegistrationRepository;
            _unitOfWork = unitOfWork;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Result<UserPendingRegistration>> ProcessAsync(
            Login login,
            string rawPassword,
            CancellationToken cancellationToken = default)
        {
            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if (ipAddressResult.IsFailure)
                return Result<UserPendingRegistration>.Failure(ipAddressResult.Errors);

            var limiterResult = await _rateLimiter.EnsureAllowedAsync(
                RateLimitAction.Registration,
                ipAddressResult.Value,
                login.Value,
                cancellationToken
            );

            if (limiterResult.IsFailure)
                return Result<UserPendingRegistration>.Failure(limiterResult.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var exists = await _userRepository.ExistsUserByLoginAsync(login, cancellationToken);

            if (exists)
                return Result<UserPendingRegistration>.Failure(UserErrors.UserAlreadyExists<RegistrationFlowCoordinator>(login.Value));

            var plainPasswordResult = PlainPassword.Create(rawPassword);

            if (plainPasswordResult.IsFailure)
                return Result<UserPendingRegistration>.Failure(plainPasswordResult.Errors);

            PasswordHash passwordHash = _passwordHasher.HashPassword(plainPasswordResult.Value);

            var password = Password.Create(passwordHash);

            if (password.IsFailure)
                return Result<UserPendingRegistration>.Failure(password.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var existingRegistration = await _userPendingRegistrationRepository.GetPendingUserByLoginAsync(login, cancellationToken);

            UserPendingRegistration registration;

            if(existingRegistration is not null)
            {
                existingRegistration.Refresh(password.Value, _clock.UtcNow);

                await _unitOfWork.ExecuteAsync(async ct =>
                {
                    await _userPendingRegistrationRepository.UpdatePendingUserAsync(existingRegistration, ct);
                }, cancellationToken);

                registration = existingRegistration;
            }
            else
            {
                var createResult = UserPendingRegistration.Create(login, password.Value, _clock.UtcNow);

                if (createResult.IsFailure)
                    return Result<UserPendingRegistration>.Failure(createResult.Errors);

                registration = createResult.Value;

                await _unitOfWork.ExecuteAsync(async ct =>
                {
                    await _userPendingRegistrationRepository.CreatePendingUserAsync(registration, ct);
                }, cancellationToken);
            }

            await _domainEventDispatcher.DispatchAsync(registration.DomainEvents, cancellationToken);

            registration.ClearDomainEvents();

            return Result<UserPendingRegistration>.Success(registration);
        }
    }
}
