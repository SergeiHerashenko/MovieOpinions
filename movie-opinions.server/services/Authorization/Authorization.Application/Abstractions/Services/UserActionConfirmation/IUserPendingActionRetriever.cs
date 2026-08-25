using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Application.Abstractions.Services.UserActionConfirmation
{
    public interface IUserPendingActionRetriever
    {
        Task<Result<UserPendingAction>> RetrieverAsync<TAction>(
            UserId userId,
            string confirmationToken,
            CancellationToken cancellationToken = default)
            where TAction : UserAction;
    }
}
