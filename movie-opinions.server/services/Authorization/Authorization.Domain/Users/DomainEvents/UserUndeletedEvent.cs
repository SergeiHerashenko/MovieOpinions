using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents
{
    public sealed class UserUndeletedEvent : DomainEvent
    {
        public UserDeletionId UserDeletionId { get; }

        public Login Login { get; }

        public DateTimeOffset UndeleteTime { get; }

        public UserUndeletedEvent(UserDeletionId userDeletionId, Login login, DateTimeOffset undeleteTime)
            : base(undeleteTime)
        {
            UserDeletionId = userDeletionId;
            Login = login;
            UndeleteTime = undeleteTime;
        }
    }
}
