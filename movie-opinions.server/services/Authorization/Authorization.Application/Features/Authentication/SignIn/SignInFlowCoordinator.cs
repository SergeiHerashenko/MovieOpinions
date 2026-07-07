using Authorization.Application.Common.ApplicationErrors.Users;
using Authorization.Application.Common.Enums;
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
        private readonly IAccessService _accessService;

        private readonly IUserRepository _userRepository;

        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public SignInFlowCoordinator(
            IRateLimiter rateLimiter,
            IUserContext userContext,
            IHasher hasher,
            IAccessService accessService,
            IUserRepository userRepository,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _rateLimiter = rateLimiter;
            _userContext = userContext;
            _hasher = hasher;
            _accessService = accessService;
            _userRepository = userRepository;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Result> ProcessAsync(
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

                return Result.Failure(UserErrors.NotFound<SignInFlowCoordinator>(login.Value));
            }

            var chekPassword = _hasher.Verify(rawPassword, existingUser.Password.HashPassword);

            if (!chekPassword)
                return Result.Failure(UserErrors.InvalidPassword<SignInFlowCoordinator>(login.Value));

            var access = await _accessService.CheckUserAccess(existingUser);
        }
    }
}
