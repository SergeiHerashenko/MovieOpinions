namespace Authorization.Domain.DomainEvents.UserRestriction
{
    public class UserRestrictionRequestedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string? Reason { get; }

        public string NameBannedBy { get; }

        public DateTime? ExpiresAt { get; }

        public DateTime CreatedAt { get; }

        public UserRestrictionRequestedEvent(Guid userId, string? reason, string nameBannedBy, DateTime? expiresAt, DateTime createdAt)
            : base(createdAt)
        {
            UserId = userId;
            Reason = reason;
            NameBannedBy = nameBannedBy;
            ExpiresAt = expiresAt;
        }
    }
}
