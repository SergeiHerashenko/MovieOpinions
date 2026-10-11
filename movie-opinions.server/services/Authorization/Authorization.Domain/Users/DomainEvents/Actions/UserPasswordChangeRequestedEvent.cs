using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents.Actions
{
    public sealed class UserPasswordChangeRequestedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public UserPendingActionId UserPendingActionId { get; }

        public Login Login { get; }

        public ConfirmationFlowToken ConfirmationFlowToken { get; }

        public DateTimeOffset ExpiresAt { get; }

        public UserPasswordChangeRequestedEvent(
            UserId userId,
            UserPendingActionId userPendingActionId,
            Login login,
            ConfirmationFlowToken confirmationFlowToken,
            DateTimeOffset expiresAt,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserId = userId;
            UserPendingActionId = userPendingActionId;
            Login = login;
            ConfirmationFlowToken = confirmationFlowToken;
            ExpiresAt = expiresAt;
        }
    }
}
