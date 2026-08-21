using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserActionEvent : DomainEvent
    {
        public UserPendingActionId UserPendingActionId { get; }

        public Login Login { get; }

        public UserAction UserAction { get; }

        public DateTimeOffset ExpiresAt { get; }

        public UserActionEvent(UserPendingActionId userPendingActionId, Login login, UserAction userAction, DateTimeOffset expiresAt, DateTimeOffset now)
            : base(now)
        {
            UserPendingActionId = userPendingActionId;
            Login = login;
            UserAction = userAction;
            ExpiresAt = expiresAt;
        }
    }
}
