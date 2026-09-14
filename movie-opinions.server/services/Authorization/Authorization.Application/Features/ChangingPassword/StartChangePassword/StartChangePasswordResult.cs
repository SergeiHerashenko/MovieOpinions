using Authorization.Application.Features.Services.UserActiveContacts.Models;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;

namespace Authorization.Application.Features.ChangingPassword.StartChangePassword
{
    public sealed class StartChangePasswordResult
    {
        public ConfirmationFlowToken ConfirmationToken { get; }

        public IReadOnlyCollection<ActiveContactChannel> Channels { get; }

        public StartChangePasswordResult(
            ConfirmationFlowToken confirmationToken,
            IReadOnlyCollection<ActiveContactChannel> channels)
        {
            ConfirmationToken = confirmationToken;
            Channels = channels;
        }
    }
}
