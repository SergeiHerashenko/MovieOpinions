using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.Services.UserActionConfirmation;
using Authorization.Application.Common.Errors.Users;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Application.Features.Services.UserActionConfirmation
{
    public sealed class UserPendingActionRetriever : IUserPendingActionRetriever
    {
        private readonly IUserRepository _userRepository;
        private readonly IClock _clock;

        public UserPendingActionRetriever(
            IUserRepository userRepository,
            IClock clock)
        {
            _userRepository = userRepository;
            _clock = clock;
        }

        public async Task<Result<UserPendingAction>> RetrieverAsync<TAction>(
            UserId userId, 
            string confirmationToken, 
            CancellationToken cancellationToken = default) 
            where TAction : UserAction
        {
            var existingUser = await _userRepository.GetUserByIdAsync(userId, cancellationToken);

            if (existingUser is null)
                return Result<UserPendingAction>.Failure(UserErrors.NotFound<UserPendingActionRetriever>(userId.Value.ToString()));

            var tokenConfirmation = ConfirmationFlowToken.Restore(confirmationToken);

            var actionResult = existingUser.GetActionForConfirmation<TAction>(tokenConfirmation, _clock.UtcNow);

            if (actionResult.IsFailure)
                return Result<UserPendingAction>.Failure(actionResult.Errors);

            return actionResult;
        }
    }
}
