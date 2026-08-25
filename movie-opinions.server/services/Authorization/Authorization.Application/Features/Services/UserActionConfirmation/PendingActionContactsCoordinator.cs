using Authorization.Application.Abstractions.AggregateChanges;
using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.Services.UserActionConfirmation;
using Authorization.Application.Abstractions.Services.UserActiveContacts;
using Authorization.Application.Common.Errors.Users;
using Authorization.Application.Features.Services.UserActiveContacts.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Features.Services.UserActionConfirmation
{
    public sealed class PendingActionContactsCoordinator : IPendingActionContactsCoordinator
    {
        private readonly ILogger<PendingActionContactsCoordinator> _logger;

        private readonly IActiveContactsProvider _activeContactsProvider;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        private readonly IClock _clock;

        private readonly IAggregateChangesDispatcher _aggregateChangesDispatcher;

        public PendingActionContactsCoordinator(
            ILogger<PendingActionContactsCoordinator> logger,
            IActiveContactsProvider activeContactsProvider,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IClock clock,
            IAggregateChangesDispatcher aggregateChangesDispatcher)
        {
            _logger = logger;
            _activeContactsProvider = activeContactsProvider;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _clock = clock;
            _aggregateChangesDispatcher = aggregateChangesDispatcher;
        }

        public async Task<Result<IReadOnlyCollection<ActiveContactChannel>>> GetAsync(
            UserId userId, 
            UserPendingActionId actionId, 
            CancellationToken cancellationToken = default)
        {
            var activeChannelsResult = await _activeContactsProvider.GetActiveContactsAsync(
                userId,
                cancellationToken
            );

            if (activeChannelsResult.IsFailure)
            {
                var failResult = await MarkPendingActionAsFailedAsync(
                    userId,
                    actionId,
                    cancellationToken
                );

                if (failResult.IsFailure)
                {
                    _logger.LogCritical(
                        "Failed to compensate pending action {ActionId} for user {UserId} after active contacts retrieval failure!",
                        actionId.Value,
                        userId.Value
                    );

                    return Result<IReadOnlyCollection<ActiveContactChannel>>.Failure(failResult.Errors);
                }

                return Result<IReadOnlyCollection<ActiveContactChannel>>.Failure(activeChannelsResult.Errors);
            }

            return Result<IReadOnlyCollection<ActiveContactChannel>>.Success(activeChannelsResult.Value);
        }

        private async Task<Result> MarkPendingActionAsFailedAsync(
            UserId userId,
            UserPendingActionId actionId,
            CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.ExecuteAsync(async ct =>
            {
                var currentUser = await _userRepository.GetUserByIdForUpdateAsync(userId, ct);

                if (currentUser is null)
                    return Result.Failure(UserErrors.NotFound<PendingActionContactsCoordinator>(userId.Value.ToString()));

                var failResult = currentUser.FailPendingAction(actionId, _clock.UtcNow);

                if (failResult.IsFailure)
                    return Result.Failure(failResult.Errors);

                await _aggregateChangesDispatcher.DispatchAsync(currentUser.AggregateChanges, ct);

                currentUser.ClearAggregateChanges();

                return Result.Success();
            }, cancellationToken);
        }
    }
}
