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

        public DateTimeOffset? ExpiresAt { get; private set; }

        public bool IsActive { get; private set; }

        #region Creation
        private UserRestriction(Guid userId, string login, string? reason, string nameBannedBy, DateTimeOffset? expiresAt)
           : base()
        {
            if (userId == Guid.Empty)
                throw new ValidationDomainException(
                    DomainErrorCodes.IdentifierError.Empty,
                    $"{nameof(userId)} validation failed: value is null. Entity {nameof(UserRestriction)}!"
                );

            if (login is null)
                throw new ValidationDomainException(
                    DomainErrorCodes.LoginError.Empty,
                    $"{nameof(login)} validation failed: value is null. Entity {nameof(UserRestriction)}!"
                );

            UserId = userId;
            Login = login;
            Reason = reason;
            NameBannedBy = nameBannedBy;
            ExpiresAt = expiresAt;
            IsActive = true;
        }

        public static UserRestriction CreateRestrictionForUser(User user, string? reason, string nameBannedBy, DateTimeOffset? expiresAt, DateTimeOffset now)
        {
            if (user.IsBlocked)
                throw new BusinessRuleViolationDomainException(
                    DomainErrorCodes.AccountStatusError.Blocked, 
                    $"User is already blocked and cannot receive additional restrictions. Entity {nameof(UserRestriction)}!"
                );

            if (expiresAt.HasValue && expiresAt.Value <= now)
                throw new BusinessRuleViolationDomainException(
                    DomainErrorCodes.GeneralError.OperationNotAllowed, 
                    $"Restriction expiration date must be set to a future time. Entity {nameof(UserRestriction)}!"
                );

            var userRestriction = new UserRestriction(user.Id, user.Login.Value, reason, nameBannedBy, expiresAt);

            userRestriction.AddDomainEvent(new 
                UserRestrictionRequestedEvent(user.Id, reason, nameBannedBy, expiresAt, now)
            );

            return userRestriction;
        }
        #endregion

        #region Restore
        private UserRestriction(
            Guid id,
            Guid userId,
            string login,
            string? reason,
            string nameBannedBy,
            DateTimeOffset createdAt,
            DateTimeOffset? expiresAt,
            bool isActive)
            : base(id, createdAt)
        {
            if (userId == Guid.Empty)
                throw new DataInconsistencyDomainException(
                    DomainErrorCodes.RestoreError.NullReference,
                    $"Missing required field {nameof(userId)} during {nameof(UserRestriction)} entity reconstruction!"
                );

            if (login is null)
                throw new DataInconsistencyDomainException(
                    DomainErrorCodes.RestoreError.NullReference,
                    $"Missing required field {nameof(login)} during {nameof(UserRestriction)} entity reconstruction!"
                );

            UserId = userId;
            Login = login;
            Reason = reason;
            NameBannedBy = nameBannedBy;
            ExpiresAt = expiresAt;
            IsActive = isActive;
        }

        public static UserRestriction Restore(Guid id,
            Guid userId,
            string login,
            string? reason,
            string nameBannedBy,
            DateTimeOffset createdAt,
            DateTimeOffset? expiresAt,
            bool isActive)
        {
            return new UserRestriction(id, userId, login, reason, nameBannedBy, createdAt, expiresAt, isActive);
        }
        #endregion

        #region Behavior
        public void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;
        }

        public bool IsExpired(DateTimeOffset now) => ExpiresAt.HasValue && now >= ExpiresAt.Value;

        public bool IsCurrentlyActive(DateTimeOffset now) => IsActive && !IsExpired(now);
        #endregion
    }
}