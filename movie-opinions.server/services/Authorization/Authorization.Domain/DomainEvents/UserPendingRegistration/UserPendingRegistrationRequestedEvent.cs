namespace Authorization.Domain.DomainEvents.UserPendingRegistration
{
    public class UserPendingRegistrationRequestedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string? Login {  get; }

        public UserPendingRegistrationRequestedEvent(Guid userId, string? login, DateTime dateTime)
            : base(dateTime)
        {
            UserId = userId;
            Login = login;
        }
    }
}
