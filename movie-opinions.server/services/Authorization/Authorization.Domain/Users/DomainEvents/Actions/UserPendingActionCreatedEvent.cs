using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents.Actions
{
    public sealed class UserPendingActionCreatedEvent : DomainEvent
    {
        public UserPendingActionId Id { get; }

        public Login Login { get; }

        public UserAction UserAction { get; }

        public DateTimeOffset ExpiresAt { get; }

        public UserPendingActionCreatedEvent(
            UserPendingActionId id,
            Login login,
            UserAction userAction,
            DateTimeOffset expiresAt,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            Id = id;
            Login = login;
            UserAction = userAction;
            ExpiresAt = expiresAt;
        }
    }
}
