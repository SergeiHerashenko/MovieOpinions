using Authorization.Application.Features.Services.UserActiveContacts.Models;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;

namespace Authorization.Application.Features.DeletingUser.StartDeletingUser
{
    public sealed class StartDeletingUserResult
    {
        public ConfirmationFlowToken ConfirmationToken { get; }

        public IReadOnlyCollection<ActiveContactChannel> Channels { get; }

        public StartDeletingUserResult(
            ConfirmationFlowToken confirmationToken, 
            IReadOnlyCollection<ActiveContactChannel> channels)
        {
            ConfirmationToken = confirmationToken;
            Channels = channels;
        }
    }
}
