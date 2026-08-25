using Authorization.Application.Abstractions.AggregateChanges;
using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Events;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.Security.Access;
using Authorization.Application.Abstractions.Services;
using Authorization.Application.Abstractions.Services.UserPassword;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Errors.Users;
using Authorization.Application.Features.SignIn.Marker;
using Authorization.Application.Features.SignIn.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.Features.SignIn
{
    public class SignInFlowCoordinator
    {
        private readonly IRateLimiter _rateLimiter;
        private readonly IUserContext _userContext;
        private readonly IClock _clock;
        private readonly ITokenService _tokenService;
        private readonly IAccessService<ISignInMarker> _accessService;

        private readonly IUserRepository _userRepository;
        private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IUserPasswordAttemptService _userPasswordAttemptService;

        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly IAggregateChangesDispatcher _aggregateChangesDispatcher;

        public SignInFlowCoordinator(
            IRateLimiter rateLimiter,
            IUserContext userContext,
            IClock clock,
            ITokenService tokenService,
            IAccessService<ISignInMarker> accessService,
            IUserRepository userRepository,
            IUserRefreshTokenRepository userRefreshTokenRepository,
            IUnitOfWork unitOfWork,
            IUserPasswordAttemptService userPasswordAttemptService,
            IDomainEventDispatcher domainEventDispatcher,
            IAggregateChangesDispatcher aggregateChangesDispatcher)
        {
            _rateLimiter = rateLimiter;
            _userContext = userContext;
            _clock = clock;
            _tokenService = tokenService;
            _accessService = accessService;
            _userRepository = userRepository;
            _userRefreshTokenRepository = userRefreshTokenRepository;
            _unitOfWork = unitOfWork;
            _userPasswordAttemptService = userPasswordAttemptService;
            _domainEventDispatcher = domainEventDispatcher;
            _aggregateChangesDispatcher = aggregateChangesDispatcher;
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

            var existingUserResult = await _userPasswordAttemptService.ProcessAsync(login, rawPassword, cancellationToken);

            if (existingUserResult.IsFailure)
                return Result<SignInResult<Guid>>.Failure(existingUserResult.Errors);

            var verifiedUser = existingUserResult.Value;
            var verifiedPasswordHash = verifiedUser.Password.Value;

            var signInTransaction = await _unitOfWork.ExecuteAsync(async ct =>
            {
                var currentUser = await _userRepository.GetUserByIdForUpdateAsync(verifiedUser.Id, ct);

                if (currentUser is null)
                    return Result<SignInTransactionResult>.Failure(UserErrors.NotFound<SignInFlowCoordinator>(verifiedUser.Login.Value));

                if (!string.Equals(
                        currentUser.Password.Value,
                        verifiedPasswordHash,
                        StringComparison.Ordinal))
                {
                    return Result<SignInTransactionResult>.Failure(UserErrors.CredentialsChanged<SignInFlowCoordinator>());
                }
                    
                var accessResult = await _accessService.CheckUserAccess(currentUser);

                if (accessResult.IsFailure)
                    return Result<SignInTransactionResult>.Failure(accessResult.Errors);

                var loginSuccess = currentUser.LoginSuccess(_clock.UtcNow);

                if (loginSuccess.IsFailure)
                    return Result<SignInTransactionResult>.Failure(loginSuccess.Errors);

                var userToken = _tokenService.CreateUserSession(currentUser, ct);

                if (userToken.IsFailure)
                    return Result<SignInTransactionResult>.Failure(userToken.Errors);

                await _userRepository.UpdateUserAsync(currentUser, ct);

                await _userRefreshTokenRepository.CreateRefreshTokenAsync(userToken.Value.UserRefreshToken, ct);

                await _aggregateChangesDispatcher.DispatchAsync(currentUser.AggregateChanges, ct);

                currentUser.ClearAggregateChanges();

                var result = new SignInTransactionResult(currentUser, userToken.Value);

                return Result<SignInTransactionResult>.Success(result);
            }, cancellationToken);

            if (signInTransaction.IsFailure)
                return Result<SignInResult<Guid>>.Failure(signInTransaction.Errors);

            var currentUser = signInTransaction.Value.User;
            var userToken = signInTransaction.Value.TokenResponse;

            await _domainEventDispatcher.DispatchAsync(currentUser.DomainEvents, cancellationToken);

            currentUser.ClearDomainEvents();

            var signInResult = SignInResult.Success<Guid>(
                currentUser.Id,
                currentUser.Role,
                userToken.AccessToken,
                userToken.UserRefreshToken.RefreshToken.Value
            );

            return Result<SignInResult<Guid>>.Success(signInResult);
        }
    }
}
