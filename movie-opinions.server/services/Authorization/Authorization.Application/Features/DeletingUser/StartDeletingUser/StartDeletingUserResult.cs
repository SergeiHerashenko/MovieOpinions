using Authorization.Application.Features.DeletingUser.StartDeletingUser.Model;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;

namespace Authorization.Application.Features.DeletingUser.StartDeletingUser
{
    public class StartDeletingUserResult
    {
        public ConfirmationToken ConfirmationToken { get; }

        public IReadOnlyCollection<AvailableDeletionChannel> Channels { get; }

        public StartDeletingUserResult(ConfirmationToken confirmationToken, IReadOnlyCollection<AvailableDeletionChannel> channels)
        {
            ConfirmationToken = confirmationToken;
            Channels = channels;
        }
    }
}
