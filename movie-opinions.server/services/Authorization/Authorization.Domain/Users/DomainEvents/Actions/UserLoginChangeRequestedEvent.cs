using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents.Actions
{
    public sealed class UserLoginChangeRequestedEvent : DomainEvent
    {
        public UserId UserId { get; }

        public UserPendingActionId UserPendingActionId { get; }

        public Login CurrentLogin { get; }

        public Login RequestedLogin { get; }

        public ConfirmationFlowToken ConfirmationFlowToken { get; }

        public DateTimeOffset ExpiresAt { get; }

        public UserLoginChangeRequestedEvent(
            UserId userId,
            UserPendingActionId userPendingActionId,
            Login currentLogin,
            Login requestedLogin,
            ConfirmationFlowToken confirmationFlowToken,
            DateTimeOffset expiresAt,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserId = userId;
            UserPendingActionId = userPendingActionId;
            CurrentLogin = currentLogin;
            RequestedLogin = requestedLogin;
            ConfirmationFlowToken = confirmationFlowToken;
            ExpiresAt = expiresAt;
        }
    }
}
