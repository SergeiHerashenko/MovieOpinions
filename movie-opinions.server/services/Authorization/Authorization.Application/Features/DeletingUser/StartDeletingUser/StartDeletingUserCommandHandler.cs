using Authorization.Application.Abstractions.AggregateChanges;
using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Events;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.Services.UserActionConfirmation;
using Authorization.Application.Abstractions.Services.UserPassword;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Errors.Users;
using Authorization.Application.Features.DeletingUser.StartDeletingUser.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects;
using MediatR;

namespace Authorization.Application.Features.DeletingUser.StartDeletingUser
{
    public class StartDeletingUserCommandHandler 
        : IRequestHandler<StartDeletingUserCommand, Result<StartDeletingUserResult>>
    {
        private readonly IRateLimiter _rateLimiter;
        private readonly IUserContext _userContext;

        private readonly IUserPasswordAttemptService _userPasswordAttemptService;
        private readonly IPendingActionContactsCoordinator _pendingActionContactsCoordinator;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        private readonly IClock _clock;

        private readonly IAggregateChangesDispatcher _aggregateChangesDispatcher;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public StartDeletingUserCommandHandler(
            IRateLimiter rateLimiter,
            IUserContext userContext,
            IUserPasswordAttemptService userPasswordAttemptService,
            IPendingActionContactsCoordinator pendingActionContactsCoordinator,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
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
            _clock = clock;
            _aggregateChangesDispatcher = aggregateChangesDispatcher;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Result<StartDeletingUserResult>> Handle(
            StartDeletingUserCommand command, 
            CancellationToken cancellationToken)
        {
            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if (ipAddressResult.IsFailure)
                return Result<StartDeletingUserResult>.Failure(ipAddressResult.Errors);

            var userId = UserId.Restore(_userContext.GetUserId());

            var limiterResult = await _rateLimiter.EnsureAllowedAsync(
                RateLimitAction.ChangeDeletingUser,
                ipAddressResult.Value,
                userId.Value.ToString(),
                cancellationToken
            );

            if (limiterResult.IsFailure)
                return Result<StartDeletingUserResult>.Failure(limiterResult.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var existingUserResult = await _userPasswordAttemptService.ProcessAsync(
                userId,
                command.Password,
                cancellationToken
            );

            if (existingUserResult.IsFailure)
                return Result<StartDeletingUserResult>.Failure(existingUserResult.Errors);

            var verifiedUser = existingUserResult.Value;
            var verifiedPasswordHash = existingUserResult.Value.Password.Value;

            cancellationToken.ThrowIfCancellationRequested();

            var startResult = await StartDeletionAsync(
                verifiedUser.Id,
                verifiedPasswordHash,
                command.Reason,
                cancellationToken
            );

            if (startResult.IsFailure)
                return Result<StartDeletingUserResult>.Failure(startResult.Errors);

            var startedDeletion = startResult.Value;

            var channelsResult = await _pendingActionContactsCoordinator.GetAsync(
                startedDeletion.User.Id,
                startedDeletion.Action.Id,
                cancellationToken
            );

            if (channelsResult.IsFailure)
                return Result<StartDeletingUserResult>.Failure(channelsResult.Errors);

            var response = new StartDeletingUserResult(startedDeletion.Action.ConfirmationToken, channelsResult.Value);

            return Result<StartDeletingUserResult>.Success(response);
        }

        private async Task<Result<StartedDeletion>> StartDeletionAsync(
            UserId userId,
            string verifiedPasswordHash,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ExecuteAsync(async ct =>
            {
                var currentUser = await _userRepository.GetUserByIdForUpdateAsync(userId, ct);

                if(currentUser is null)
                    return Result<StartedDeletion>.Failure(UserErrors.NotFound<StartDeletingUserCommandHandler>(userId.Value.ToString()));

                if(!string.Equals(
                        currentUser.Password.Value,
                        verifiedPasswordHash,
                        StringComparison.Ordinal))
                {
                    return Result<StartedDeletion>.Failure(UserErrors.CredentialsChanged<StartDeletingUserCommandHandler>());
                }

                var deletion = currentUser.GetDeletion();

                if (deletion.IsSuccess)
                    return Result<StartedDeletion>.Failure(UserErrors.UserIsDeleted<StartDeletingUserCommandHandler>(deletion.Value.RestoreUntil));

                var actionResult = currentUser.ActionDeletingUser(reason, _clock.UtcNow);

                if (actionResult.IsFailure)
                    return Result<StartedDeletion>.Failure(actionResult.Errors);

                await _aggregateChangesDispatcher.DispatchAsync(currentUser.AggregateChanges, ct);

                currentUser.ClearAggregateChanges();

                return Result<StartedDeletion>.Success(
                    new StartedDeletion(
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
