namespace Authorization.Domain.DomainEvents.UserRestriction
{
    public class UserRestrictionRequestedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public string? Reason { get; }

        public string NameBannedBy { get; }

        public DateTimeOffset? ExpiresAt { get; }

        public DateTimeOffset CreatedAt { get; }

        public UserRestrictionRequestedEvent(Guid userId, string? reason, string nameBannedBy, DateTimeOffset? expiresAt, DateTimeOffset createdAt)
            : base(createdAt)
        {
            UserId = userId;
            Reason = reason;
            NameBannedBy = nameBannedBy;
            ExpiresAt = expiresAt;
        }
    }
}
