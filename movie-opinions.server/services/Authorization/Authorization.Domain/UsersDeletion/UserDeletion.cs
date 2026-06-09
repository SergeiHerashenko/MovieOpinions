using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;
using Authorization.Domain.DomainEvents.UserDeletion;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersDeletion.Enums;
using Authorization.Domain.UsersDeletion.ValueObjects;

namespace Authorization.Domain.UsersDeletion
{
    public class UserDeletion : AggregateRoot<UserDeletionId, Guid>
    {
        public UserId UserId { get; private set; }

        public Login Login { get; private set; }

        public string? Reason { get; private set; }

        public DateTimeOffset DeletedAt { get; private set; }

        public DateTimeOffset RestoreUntil { get; private set; }

        public DateTimeOffset? RestoredAt { get; private set; }

        public DeletionStatus Status { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        #region Creation
        private UserDeletion(UserDeletionId userDeletionId, UserId userId, Login login, string? reason, DateTimeOffset deletedAt) 
            : base(userDeletionId)
        {
            UserId = userId;
            Login = login;
            Reason = reason;
            DeletedAt = deletedAt;
            RestoreUntil = deletedAt.AddDays(30);
            RestoredAt = null;
            Status = DeletionStatus.Deleted;
            UpdatedAt = null;
        }

        public static Result<UserDeletion> Create(UserId userId, Login login, string? reason, DateTimeOffset deletedAt)
        {
            if (userId is null)
                return Result<UserDeletion>.Failure(UserError.Empty(nameof(userId)));

            if (login is null)
                return Result<UserDeletion>.Failure(UserError.Empty(nameof(login)));

            var userDeletion = new UserDeletion(UserDeletionId.CreateUnique(), userId, login, reason, deletedAt);

            userDeletion.AddDomainEvent(
                new UserAccountDeletionRequestedEvent(userId, login, reason, userDeletion.RestoreUntil, userDeletion.CreatedAt)
            );

            return Result<UserDeletion>.Success(userDeletion);
        }
        #endregion

        #region Restore
        private UserDeletion(
            UserDeletionId userDeletionId, 
            UserId userId, 
            Login login, 
            string? reason,
            DateTimeOffset deletedAt,
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
            DeletedAt = deletedAt;
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
            DateTimeOffset deletedAt,
            DateTimeOffset createdAt,
            DateTimeOffset restoreUntil,
            DateTimeOffset? restoredAt,
            DeletionStatus deletionStatus,
            DateTimeOffset? updatedAt)
        {
            if (userDeletionId is null)
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"Cannot restore entity {nameof(UserDeletion)} because identifier {nameof(userDeletionId)} is invalid!",
                    new Dictionary<string, object>
                    {
                        ["entity"] = nameof(UserDeletion),
                        ["field"] = nameof(userDeletionId),
                        ["operation"] = "restore"
                    }
                );

            if (userId is null)
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"Cannot restore entity {nameof(UserDeletion)} because identifier {nameof(userId)} is invalid!",
                    new Dictionary<string, object>
                    {
                        ["entity"] = nameof(UserDeletion),
                        ["field"] = nameof(userId),
                        ["operation"] = "restore"
                    }
                );

            if (login is null)
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"Cannot restore entity {nameof(UserDeletion)} because identifier {nameof(login)} is invalid!",
                    new Dictionary<string, object>
                    {
                        ["entity"] = nameof(UserDeletion),
                        ["field"] = nameof(login),
                        ["operation"] = "restore"
                    }
                );

            return new UserDeletion(userDeletionId, userId, login, reason, deletedAt, createdAt, restoreUntil, restoredAt, deletionStatus, updatedAt);
        }
        #endregion

        #region Behavior
        public Result Undelete(DateTimeOffset now)
        {
            if (Status == DeletionStatus.Restored)
                return Result.Failure(UserError.NoChangesDetected("Account has already been restored!"));

            if (now >= RestoreUntil)
                return Result.Failure(UserError.RestoreIsNotAllowed(RestoreUntil));

            Status = DeletionStatus.Restored;
            RestoredAt = now;
            UpdatedAt = now;

            return Result.Success();
        }

        public void MarkAsExpired(DateTimeOffset now)
        {
            if (Status == DeletionStatus.Restored)
                return;

            if (now <= RestoreUntil)
                return;

            Status = DeletionStatus.PermanentlyDeleted;
            UpdatedAt = now;
        }
        #endregion
    }
}
