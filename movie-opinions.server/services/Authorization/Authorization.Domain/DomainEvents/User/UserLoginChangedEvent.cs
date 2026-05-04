using Authorization.Domain.Enums;

namespace Authorization.Domain.DomainEvents.User
{
    public class UserLoginChangedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string OldLogin { get; }

        public string NewLogin { get; }

        public LoginType NewLoginType { get; }

        public UserLoginChangedEvent(Guid userId, string oldLogin, string newLogin, LoginType newLoginType, DateTime dateTime)
            : base(dateTime)
        {
            UserId = userId;
            OldLogin = oldLogin;
            NewLogin = newLogin;
            NewLoginType = newLoginType;
        }
    }
}
