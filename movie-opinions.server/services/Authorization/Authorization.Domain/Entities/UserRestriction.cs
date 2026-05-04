using Authorization.Domain.Common;
using Authorization.Domain.DomainEvents.UserRestriction;
using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;

namespace Authorization.Domain.Entities
{
    public class UserRestriction : AggregateRoot
    {
        public Guid UserId { get; private set; }

        public string Login { get; private set; }

        public string? Reason { get; private set; }

        public string NameBannedBy { get; private set; }

        public DateTime? ExpiresAt { get; private set; }

        public bool IsActive { get; private set; }

        private UserRestriction(Guid userId, string login, string? reason, string nameBannedBy, DateTime? expiresAt)
            : base()
        {
            UserId = userId;
            Login = login;
            Reason = reason;
            NameBannedBy = nameBannedBy;
            ExpiresAt = expiresAt;
            IsActive = true;
        }

        private UserRestriction(Guid id,
            Guid userId,
            string login,
            string? reason,
            string nameBannedBy,
            DateTime createdAt,
            DateTime? expiresAt,
            bool isActive)
            : base(id, createdAt)
        {
            UserId = userId;
            Login = login;
            Reason = reason;
            NameBannedBy = nameBannedBy;
            ExpiresAt = expiresAt;
            IsActive = isActive;
        }

        public static UserRestriction CreateRestrictionForUser(User user, string? reason, string nameBannedBy, DateTime? expiresAt, DateTime now)
        {
            if (user.IsBlocked)
                throw new InvalidStateDomainException(ErrorCodes.AccountStatusError.Blocked, "Користувач вже має активне блокування!");

            if (expiresAt.HasValue && expiresAt.Value <= now)
                throw new BusinessRuleViolationDomainException(ErrorCodes.GeneralError.OperationNotAllowed, "Дата закінчення обмеження має бути в майбутньому.");

            var userRestriction = new UserRestriction(user.Id, user.Login.Value, reason, nameBannedBy, expiresAt);

            userRestriction.AddDomainEvent(new UserRestrictionRequestedEvent(user.Id, reason, nameBannedBy,  expiresAt, now));

            return userRestriction;
        }

        public static UserRestriction Restore(Guid id,
            Guid userId,
            string login,
            string? reason,
            string nameBannedBy,
            DateTime createdAt,
            DateTime? expiresAt,
            bool isActive)
        {
            return new UserRestriction(id, userId, login, reason, nameBannedBy, createdAt, expiresAt, isActive);
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;
        }

        public bool IsExpired(DateTime now) => ExpiresAt.HasValue && now >= ExpiresAt.Value;

        public bool IsCurrentlyActive(DateTime now) => IsActive && !IsExpired(now);
    }
}
