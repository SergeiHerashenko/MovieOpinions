using Authorization.Application.Features.Services.UserActionConfirmation.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;

namespace Authorization.Application.Abstractions.Services.UserActionConfirmation
{
    public interface IVerifyUserActionConfirmationService
    {
        Task<Result<VerifiedUserActionResult>> VerifyAsync<TAction>(
            VerifiedUserActionData data,
            CancellationToken cancellationToken = default)
            where TAction : UserAction;
    }
}
