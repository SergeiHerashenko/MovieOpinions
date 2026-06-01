using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.DomainEvents.User
{
    public class UserDeletedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public Login Login { get; }

        public DateTimeOffset DeleteTime { get; }

        public UserDeletedEvent(Guid userId, Login login, DateTimeOffset deleteTime)
            : base(deleteTime)
        {
            UserId = userId;
            Login = login;
            DeleteTime = deleteTime;
        }
    }
}
