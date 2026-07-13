using Authorization.Application.Common.ApplicationErrors.Users;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Security.Models;
using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Events;
using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Security.Services;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersRefreshToken.ValueObjects;

namespace Authorization.Application.Features.Authentication.SignIn
{
    public class SignInFlowCoordinator
    {
        private readonly IRateLimiter _rateLimiter;
        private readonly IUserContext _userContext;
        private readonly IHasher _hasher;
        private readonly IClock _clock;
        private readonly IAccessService _accessService;
        private readonly ITokenService _tokenService;

        private readonly IUserRepository _userRepository;

        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public SignInFlowCoordinator(
            IRateLimiter rateLimiter,
            IUserContext userContext,
            IHasher hasher,
            IClock clock,
            IAccessService accessService,
            ITokenService tokenService,
            IUserRepository userRepository,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _rateLimiter = rateLimiter;
            _userContext = userContext;
            _hasher = hasher;
            _clock = clock;
            _accessService = accessService;
            _tokenService = tokenService;
            _userRepository = userRepository;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Result<SignInResult<Guid>>> ProcessAsync(
            Login login,
            string rawPassword,
            CancellationToken cancellationToken = default)
        {
            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if (ipAddressResult.IsFailure)
                return Result<SignInResult<Guid>>.Failure(ipAddressResult.Errors);

            var ipAddress = ipAddressResult.Value;

            var resultLimiter = await _rateLimiter.EnsureAllowedAsync(
                RateLimitAction.Login,
                ipAddress,
                login.Value,
                cancellationToken
            );

            if (resultLimiter.IsFailure)
                return Result<SignInResult<Guid>>.Failure(resultLimiter.Errors);

            var existingUser = await _userRepository.GetUserByLoginAsync(login, cancellationToken);

            if (existingUser is null)
            {
                _hasher.FakeVerify(rawPassword);

                return Result<SignInResult<Guid>>.Failure(UserErrors.NotFound<SignInFlowCoordinator>(login.Value));
            }

            var access = await _accessService.CheckUserAccess(existingUser);

            if (access.IsFailure)
                return Result<SignInResult<Guid>>.Failure(access.Errors);

            var chekPassword = _hasher.Verify(rawPassword, existingUser.Password.Value);

            if (!chekPassword)
            {
                existingUser.RecordFailedLoginAttempt(_clock.UtcNow);
                await _userRepository.UpdateAsync(existingUser, cancellationToken);

                return Result<SignInResult<Guid>>.Failure(UserErrors.InvalidPassword<SignInFlowCoordinator>(login.Value));
            }

            existingUser.LoginSuccess(ipAddress, _clock.UtcNow);
            
            var userSessionDTO = UserSessionDTO.Create(
                existingUser.Id,
                existingUser.Login,
                existingUser.Role,
                ipAddress
            );

            var userToken = await _tokenService.CreateUserSessionAsync(userSessionDTO);

            if(userToken.IsFailure)
                return Result<SignInResult<Guid>>.Failure(userToken.Errors);

            await _userRepository.UpdateAsync(existingUser, cancellationToken);

            var signInResult = SignInResult.Success<Guid>(
                existingUser.Id,
                existingUser.Role,
                userToken.Value
            );

            return Result<SignInResult<Guid>>.Success(signInResult);
        }
    }
}
