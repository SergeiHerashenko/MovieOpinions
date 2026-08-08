using Authorization.Domain.Common.Errors.Common;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.Entities.UsersDeletion
{
    public class UserDeletion : Entity<UserDeletionId>
    {
        public UserId UserId { get; private set; }

        public Login Login { get; private set; }

        public string? Reason { get; private set; }

        public DateTimeOffset RestoreUntil { get; private set; }

        public DateTimeOffset? RestoredAt { get; private set; }

        public DeletionStatus Status { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        #region Creation
        private UserDeletion(UserDeletionId userDeletionId, UserId userId, Login login, DateTimeOffset now, string? reason = null)
            : base(userDeletionId, now)
        {
            UserId = userId;
            Login = login;
            Reason = reason;
            RestoreUntil = now.AddDays(30);
            RestoredAt = null;
            Status = DeletionStatus.Deleted;
            UpdatedAt = null;
        }

        internal static Result<UserDeletion> Create(UserId userId, Login login, DateTimeOffset now, string? reason = null)
        {
            if (userId is null)
                return Result<UserDeletion>.Failure(CommonErrors.Identifier.EmptyIdentifier<UserDeletion>(nameof(userId)));

            if (login is null)
                return Result<UserDeletion>.Failure(LoginErrors.EmptyLogin<UserDeletion>());

            var userDeletion = new UserDeletion(UserDeletionId.Create(), userId, login, now, reason);

            userDeletion.AddDomainEvent(new UserDeletedEvent(userDeletion.Id, userDeletion.Login, now));

            return Result<UserDeletion>.Success(userDeletion);
        }
        #endregion

        #region Restoration
        private UserDeletion(
            UserDeletionId userDeletionId,
            UserId userId,
            Login login,
            string? reason,
            DateTimeOffset createdAt,
            DateTimeOffset restoreUntil,
            DateTimeOffset? restoredAt,
            DeletionStatus deletionStatus,
            DateTimeOffset? updatedAt)
            : base(userDeletionId, createdAt)
        {
            UserId = userId;
            Login = login;
            Reason = reason;
            RestoreUntil = restoreUntil;
            RestoredAt = restoredAt;
            Status = deletionStatus;
            UpdatedAt = updatedAt;
        }

        public static UserDeletion Restore(
            UserDeletionId userDeletionId,
            UserId userId,
            Login login,
            string? reason,
            DateTimeOffset createdAt,
            DateTimeOffset restoreUntil,
            DateTimeOffset? restoredAt,
            DeletionStatus deletionStatus,
            DateTimeOffset? updatedAt)
        {
            DomainGuard.AgainstNull<UserDeletion>(
                (userDeletionId, nameof(userDeletionId)),
                (userId, nameof(userId)),
                (login, nameof(login)));

            ValidateState(deletionStatus, updatedAt, createdAt, restoreUntil, restoredAt);

            return new UserDeletion(userDeletionId, userId, login, reason, createdAt, restoreUntil, restoredAt, deletionStatus, updatedAt);
        }
        #endregion

        #region Behavior
        internal Result Undelete(DateTimeOffset now)
        {
            if (Status == DeletionStatus.Restored)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<UserDeletion>(nameof(Status)));

            if (now > RestoreUntil)
                return Result.Failure(DeletionErrors.Expired<UserDeletion>(nameof(RestoreUntil)));

            Status = DeletionStatus.Restored;
            RestoredAt = now;
            UpdatedAt = now;

            AddDomainEvent(new UserUndeletedEvent(Id, Login, now));

            return Result.Success();
        }

        internal bool MarkAsExpired(DateTimeOffset now)
        {
            if (Status == DeletionStatus.Restored)
                return false;

            if (now <= RestoreUntil)
                return false;

            Status = DeletionStatus.PermanentlyDeleted;
            UpdatedAt = now;

            return true;
        }
        #endregion

        #region Guadr
        private static void ValidateState(
            DeletionStatus deletionStatus,
            DateTimeOffset? updatedAt,
            DateTimeOffset createdAt,
            DateTimeOffset restoreUntil,
            DateTimeOffset? restoredAt)
        {
            if (!Enum.IsDefined(typeof(DeletionStatus), deletionStatus))
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<UserDeletion>(nameof(deletionStatus), deletionStatus.ToString());

            if (restoreUntil < createdAt)
                throw DomainDataInconsistencyException.ValueOutOfRange<UserDeletion>(nameof(restoreUntil), restoreUntil);

            if (updatedAt is not null && updatedAt < createdAt)
                throw DomainInvariantViolationException.BrokenState<UserDeletion>(
                    $"Update time '{updatedAt}' cannot be less than creation time '{createdAt}'!",
                    new Dictionary<string, object?>
                    {
                        ["CreatedAt"] = createdAt,
                        ["UpdatedAt"] = updatedAt
                    }
                );

            if (restoredAt is not null && restoredAt < createdAt)
                throw DomainInvariantViolationException.BrokenState<UserDeletion>(
                   $"The recovery time '{restoredAt}' cannot be less than creation time '{createdAt}'!",
                   new Dictionary<string, object?>
                   {
                       ["RestoredAt"] = restoredAt,
                       ["CreatedAt"] = createdAt
                   }
               );

            bool isValidState = (deletionStatus, updatedAt, restoredAt) switch
            {
                (DeletionStatus.Deleted, null, null) => true,
                (DeletionStatus.Restored, not null, not null) => true,
                (DeletionStatus.PermanentlyDeleted, not null, null) => true,
                _ => false
            };

            if (!isValidState)
            {
                throw DomainInvariantViolationException.BrokenState<UserDeletion>(
                    $"Inconsistent state: fields '{nameof(deletionStatus)}' and '{nameof(updatedAt)}' and '{nameof(restoredAt)}' are not consistent!",
                    new Dictionary<string, object?>
                    {
                        ["DeletionStatus"] = deletionStatus,
                        ["UpdatedAt"] = updatedAt,
                        ["RestoredAt"] = restoredAt
                    }
                );
            }
        }
        #endregion
    }
}
