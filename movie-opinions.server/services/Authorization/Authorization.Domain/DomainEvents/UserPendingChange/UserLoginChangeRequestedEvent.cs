namespace Authorization.Domain.DomainEvents.UserPendingChange
{
    public class UserLoginChangeRequestedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string ConfirmToken { get; }

        public UserLoginChangeRequestedEvent(Guid userId, string confirmToken, DateTime dateTime)
            : base(dateTime)
        {
            UserId = userId;
            ConfirmToken = confirmToken;
        }
    }
}
