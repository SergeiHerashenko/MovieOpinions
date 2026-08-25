using Authorization.Application.Abstractions.AggregateChanges;
using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Events;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.Security.Hashers;
using Authorization.Application.Abstractions.Services.UserActionConfirmation;
using Authorization.Application.Abstractions.Services.UserPassword;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Errors.Users;
using Authorization.Application.Features.ChangingPassword.StartChangePassword.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.PasswordUser;
using MediatR;

namespace Authorization.Application.Features.ChangingPassword.StartChangePassword
{
    public class StartChangePasswordCommandHandler
        : IRequestHandler<StartChangePasswordCommand, Result<StartChangePasswordResult>>
    {
        private readonly IRateLimiter _rateLimiter;
        private readonly IUserContext _userContext;

        private readonly IUserPasswordAttemptService _userPasswordAttemptService;
        private readonly IPendingActionContactsCoordinator _pendingActionContactsCoordinator;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        private readonly IPasswordHasher _passwordHasher;

        private readonly IClock _clock;

        private readonly IAggregateChangesDispatcher _aggregateChangesDispatcher;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public StartChangePasswordCommandHandler(
            IRateLimiter rateLimiter,
            IUserContext userContext,
            IUserPasswordAttemptService userPasswordAttemptService,
            IPendingActionContactsCoordinator pendingActionContactsCoordinator,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IClock clock,
            IAggregateChangesDispatcher aggregateChangesDispatcher,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _rateLimiter = rateLimiter;
            _userContext = userContext;
            _userPasswordAttemptService = userPasswordAttemptService;
            _pendingActionContactsCoordinator = pendingActionContactsCoordinator;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _clock = clock;
            _aggregateChangesDispatcher = aggregateChangesDispatcher;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Result<StartChangePasswordResult>> Handle(
            StartChangePasswordCommand command, 
            CancellationToken cancellationToken = default)
        {
            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if (ipAddressResult.IsFailure)
                return Result<StartChangePasswordResult>.Failure(ipAddressResult.Errors);

            var userId = UserId.Restore(_userContext.GetUserId());

            var limiterResult = await _rateLimiter.EnsureAllowedAsync(
                RateLimitAction.PasswordChange,
                ipAddressResult.Value,
                userId.Value.ToString(),
                cancellationToken
            );

            if (limiterResult.IsFailure)
                return Result<StartChangePasswordResult>.Failure(limiterResult.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var existingUserResult = await _userPasswordAttemptService.ProcessAsync(
                userId,
                command.CurrentPassword,
                cancellationToken
            );

            if (existingUserResult.IsFailure)
                return Result<StartChangePasswordResult>.Failure(existingUserResult.Errors);

            var verifiedUser = existingUserResult.Value;
            var verifiedPasswordHash = existingUserResult.Value.Password.Value;

            cancellationToken.ThrowIfCancellationRequested();

            var plainPasswordResult = PlainPassword.Create(command.NewPassword);

            if (plainPasswordResult.IsFailure)
                return Result<StartChangePasswordResult>.Failure(plainPasswordResult.Errors);

            PasswordHash passwordHash = _passwordHasher.HashPassword(plainPasswordResult.Value);

            var newPassword = Password.Create(passwordHash);

            if(newPassword.IsFailure)
                return Result<StartChangePasswordResult>.Failure(newPassword.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var startResult = await StartPasswordChangeAsync(
                verifiedUser.Id,
                verifiedPasswordHash,
                newPassword.Value,
                plainPasswordResult.Value,
                cancellationToken
            );

            if (startResult.IsFailure)
                return Result<StartChangePasswordResult>.Failure(startResult.Errors);

            var startChange = startResult.Value;

            var channelsResult = await _pendingActionContactsCoordinator.GetAsync(
                startChange.User.Id, 
                startChange.Action.Id,
                cancellationToken
            );

            if (channelsResult.IsFailure)
                return Result<StartChangePasswordResult>.Failure(channelsResult.Errors);

            var response = new StartChangePasswordResult(startChange.Action.ConfirmationToken, channelsResult.Value);

            return Result<StartChangePasswordResult>.Success(response);
        }

        private async Task<Result<StartPasswordChange>> StartPasswordChangeAsync(
            UserId userId,
            string verifiedPasswordHash,
            Password newPassword,
            PlainPassword plainNewPassword,
            CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ExecuteAsync(async ct =>
            {
                var currentUser = await _userRepository.GetUserByIdForUpdateAsync(userId, ct);

                if (currentUser is null)
                    return Result<StartPasswordChange>.Failure(UserErrors.NotFound<StartChangePasswordCommandHandler>(userId.Value.ToString()));

                if (!string.Equals(
                        currentUser.Password.Value,
                        verifiedPasswordHash,
                        StringComparison.Ordinal))
                {
                    return Result<StartPasswordChange>.Failure(UserErrors.CredentialsChanged<StartChangePasswordCommandHandler>());
                }

                var actionResult = currentUser.ActionChangePassword(
                    plainNewPassword, 
                    newPassword, 
                    _passwordHasher.VerifyPassword, 
                    _clock.UtcNow
                );

                if (actionResult.IsFailure)
                    return Result<StartPasswordChange>.Failure(actionResult.Errors);

                await _aggregateChangesDispatcher.DispatchAsync(currentUser.AggregateChanges, ct);

                currentUser.ClearAggregateChanges();

                return Result<StartPasswordChange>.Success(
                    new StartPasswordChange(
                        currentUser,
                        actionResult.Value
                    )
                );
            }, cancellationToken);

            if (result.IsSuccess)
            {
                await _domainEventDispatcher.DispatchAsync(result.Value.User.DomainEvents, cancellationToken);

                result.Value.User.ClearDomainEvents();
            }

            return result;
        }
    }
}
