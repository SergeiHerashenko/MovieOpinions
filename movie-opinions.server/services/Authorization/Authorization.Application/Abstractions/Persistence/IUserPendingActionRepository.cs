using Authorization.Domain.Users.Entities.UsersPendingAction;

namespace Authorization.Application.Abstractions.Persistence
{
    public interface IUserPendingActionRepository
    {
        Task CreateActionUserAsync(UserPendingAction entity, CancellationToken cancellationToken = default);

        Task UpdateActionUserAsync(UserPendingAction entity, CancellationToken cancellationToken = default);
    }
}
