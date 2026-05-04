namespace Authorization.Domain.DomainEvents.UserPendingChange
{
    public class UserPasswordChangeRequestedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string ConfirmToken { get; }

        public UserPasswordChangeRequestedEvent(Guid userId, string confirmToken, DateTime dateTime)
            : base(dateTime)
        {
            UserId = userId;
            ConfirmToken = confirmToken;
        }
    }
}
