namespace Authorization.Domain.DomainEvents.UserPendingChange
{
    public class UserPasswordChangeRequestedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string ConfirmToken { get; }

        public UserPasswordChangeRequestedEvent(Guid userId, string confirmToken, DateTimeOffset dateTime)
            : base(dateTime)
        {
            UserId = userId;
            ConfirmToken = confirmToken;
        }
    }
}
