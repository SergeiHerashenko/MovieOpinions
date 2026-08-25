using Authorization.Application.Features.Services.UserActiveContacts.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Application.Abstractions.Services.UserActionConfirmation
{
    public interface IPendingActionContactsCoordinator
    {
        Task<Result<IReadOnlyCollection<ActiveContactChannel>>> GetAsync(
            UserId userId,
            UserPendingActionId actionId,
            CancellationToken cancellationToken = default);
    }
}
