using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Application.Features.Services.UserActionConfirmation.Models
{
    public sealed class VerifiedUserActionResult
    {
        public UserId UserId { get; }

        public UserPendingActionId ActionId { get; }

        public VerifiedUserActionResult(
            UserId userId, 
            UserPendingActionId actionId)
        {
            UserId = userId;
            ActionId = actionId;
        }
    }
}
