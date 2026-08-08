using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Events;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.Security.Access;
using Authorization.Application.Abstractions.Security.Hashers;
using Authorization.Application.Abstractions.Services;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Errors.Users;
using Authorization.Application.Features.SignIn.Steps;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Application.Features.SignIn
{
    public class SignInFlowCoordinator
    {
        private readonly IRateLimiter _rateLimiter;
        private readonly IUserContext _userContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IClock _clock;
        private readonly ITokenService _tokenService;
        private readonly IAccessService<ISignInMarker> _accessService;

        private readonly IUserRepository _userRepository;
        private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public SignInFlowCoordinator(
            IRateLimiter rateLimiter,
            IUserContext userContext,
            IPasswordHasher passwordHasher,
            IClock clock,
            ITokenService tokenService,
            IAccessService<ISignInMarker> accessService,
            IUserRepository userRepository,
            IUserRefreshTokenRepository userRefreshTokenRepository,
            IUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _rateLimiter = rateLimiter;
            _userContext = userContext;
            _passwordHasher = passwordHasher;
            _clock = clock;
            _tokenService = tokenService;
            _accessService = accessService;
            _userRepository = userRepository;
            _userRefreshTokenRepository = userRefreshTokenRepository;
            _unitOfWork = unitOfWork;
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

            var plainPasswordResult = PlainPassword.Create(rawPassword);

            if (plainPasswordResult.IsFailure)
                return Result<SignInResult<Guid>>.Failure(plainPasswordResult.Errors);

            var existingUser = await _userRepository.GetUserByLoginAsync(login, cancellationToken);

            if (existingUser is null)
            {
                _passwordHasher.FakeVerifyPassword(plainPasswordResult.Value);

                return Result<SignInResult<Guid>>.Failure(UserErrors.NotFound<SignInFlowCoordinator>(login.Value));
            }

            var passwordResult = _passwordHasher.VerifyPassword(plainPasswordResult.Value, existingUser.Password.Value);

            if (!passwordResult)
            {
                existingUser.RecordFailedLoginAttempt(_clock.UtcNow);

                await _unitOfWork.ExecuteAsync(async ct =>
                {
                    await _userRepository.UpdateUserAsync(existingUser, ct);
                }, cancellationToken);

                return Result<SignInResult<Guid>>.Failure(UserErrors.InvalidPassword<SignInFlowCoordinator>(login.Value));
            }

            var accessResult = await _accessService.CheckUserAccess(existingUser);

            if (accessResult.IsFailure)
                return Result<SignInResult<Guid>>.Failure(accessResult.Errors);

            var loginSuccess = existingUser.LoginSuccess(_clock.UtcNow);

            if (loginSuccess.IsFailure)
                return Result<SignInResult<Guid>>.Failure(loginSuccess.Errors);

            var userToken = _tokenService.CreateUserSessionAsync(existingUser, cancellationToken);

            if (userToken.IsFailure)
                return Result<SignInResult<Guid>>.Failure(userToken.Errors);

            await _unitOfWork.ExecuteAsync(async ct =>
            {
                await _userRepository.UpdateUserAsync(existingUser, ct);

                await _userRefreshTokenRepository.CreateRefreshTokenAsync(userToken.Value.UserRefreshToken, ct);
            }, cancellationToken);

            foreach (var domainEvent in existingUser.DomainEvents)
                await _domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);

            existingUser.ClearDomainEvents();

            var signInResult = SignInResult.Success<Guid>(
                existingUser.Id,
                existingUser.Role,
                userToken.Value.AccessToken,
                userToken.Value.UserRefreshToken.RefreshToken.Value
            );

            return Result<SignInResult<Guid>>.Success(signInResult);
        }
    }
}
