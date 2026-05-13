using Authorization.Domain.Common;
using Authorization.Domain.DomainEvents.UserDeletion;
using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;

namespace Authorization.Domain.Entities
{
    public class UserDeletion : AggregateRoot
    {
        public Guid UserId { get; private set; }

        public string Login {  get; private set; }

        public string? Reason { get; private set; }

        #region Creation
        private UserDeletion(Guid userId, string login, string? reason) : base()
        {
            if (userId == Guid.Empty)
                throw new ValidationDomainException(
                    DomainErrorCodes.IdentifierError.Empty, 
                    $"{nameof(userId)} validation failed: value is null. Entity {nameof(UserDeletion)}!"
                );

            if (login is null)
                throw new ValidationDomainException(
                    DomainErrorCodes.LoginError.Empty,
                    $"{nameof(login)} validation failed: value is null. Entity {nameof(UserDeletion)}!"
                );

            UserId = userId;
            Login = login;
            Reason = reason;
        }

        public static UserDeletion CreateForDeletedUser(User user, string? reason, DateTimeOffset now)
        {
            if (user.IsDeleted)
                throw new BusinessRuleViolationDomainException(
                    DomainErrorCodes.AccountStatusError.Deleted,
                    $"Operation not allowed for deleted user. Entity {nameof(UserDeletion)}!"
                );

            var userDeletion = new UserDeletion(user.Id, user.Login.Value, reason);

            userDeletion.AddDomainEvent(
                new UserAccountDeletionRequestedEvent(user.Id, user.Login.Value, reason, now)
            );

            return userDeletion;
        }
        #endregion

        #region Restore
        private UserDeletion(Guid id, Guid userId, string login, string? reason, DateTimeOffset createdAt)
            : base(id, createdAt)
        {
            if (userId == Guid.Empty)
                throw new DataInconsistencyDomainException(
                    DomainErrorCodes.RestoreError.NullReference,
                    $"Missing required field {nameof(userId)} during {nameof(UserDeletion)} entity reconstruction!"
                );

            if (login is null)
                throw new DataInconsistencyDomainException(
                    DomainErrorCodes.RestoreError.NullReference,
                    $"Missing required field {nameof(login)} during {nameof(UserDeletion)} entity reconstruction!"
                );

            UserId = userId;
            Login = login;
            Reason = reason;
        }

        public static UserDeletion Restore(Guid id, Guid userId, string login, string? reason, DateTimeOffset createdAt)
        {
            return new UserDeletion(id, userId, login, reason, createdAt);
        }
        #endregion
    }
}