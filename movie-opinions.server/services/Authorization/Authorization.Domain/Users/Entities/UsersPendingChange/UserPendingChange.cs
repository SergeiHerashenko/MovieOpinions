using Authorization.Domain.Common.Errors.Common;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersPendingChange.Changes;
using Authorization.Domain.Users.Entities.UsersPendingChange.Enums;
using Authorization.Domain.Users.Entities.UsersPendingChange.ValueObjects;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Entities.UsersPendingChange
{
    public class UserPendingChange : Entity<UserPendingChangeId>
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromMinutes(30);

        public UserId UserId { get; private set; }

        public ConfirmationToken ConfirmationToken { get; private set; }

        public UserChange UserChange { get; private set; }

        public DateTimeOffset ExpiresAt { get; private set; }

        public DateTimeOffset? ConfirmationTime { get; private set; }

        public DateTimeOffset? ExpiredAt { get; private set; }

        public ChangeStatus Status { get; private set; }

        #region Creation
        private UserPendingChange(
            UserPendingChangeId userPendingChangeId, 
            UserId userId, 
            ConfirmationToken confirmToken, 
            UserChange userChange,
            DateTimeOffset now)
            : base(userPendingChangeId, now)
        {
            UserId = userId;
            ConfirmationToken = confirmToken;
            UserChange = userChange;
            ExpiresAt = now.Add(ExpirationTime);
            ConfirmationTime = null;
            ExpiredAt = null;
            Status = ChangeStatus.Active;
        }

        internal static Result<UserPendingChange> Create(UserId userId, UserChange userChange, DateTimeOffset now)
        {
            if (userId is null)
                return Result<UserPendingChange>.Failure(CommonErrors.Identifier.EmptyIdentifier<UserPendingChange>(nameof(userId)));

            if (userChange is null)
                return Result<UserPendingChange>.Failure(ChangeErrors.EmptyChangeUser<UserPendingChange>());

            var pendingChange = new UserPendingChange(
                UserPendingChangeId.Create(),
                userId,
                ConfirmationToken.Create(),
                userChange,
                now
            );

            pendingChange.AddDomainEvent(new UserPendingChangeEvent(
                pendingChange.UserId,
                pendingChange.UserChange,
                pendingChange.ExpiresAt,
                pendingChange.CreatedAt)
            );

            return Result<UserPendingChange>.Success(pendingChange);
        }
        #endregion

        #region Restoration
        private UserPendingChange(
            UserPendingChangeId userPendingChangeId,
            UserId userId,
            ConfirmationToken confirmToken,
            UserChange userChange,
            DateTimeOffset expiresAt,
            DateTimeOffset? confirmationTime,
            DateTimeOffset? expiredAt,
            ChangeStatus changeStatus,
            DateTimeOffset createdAt)
            : base(userPendingChangeId, createdAt)
        {
            UserId = userId;
            ConfirmationToken = confirmToken;
            UserChange = userChange;
            ExpiresAt = expiresAt;
            ConfirmationTime = confirmationTime;
            ExpiredAt = expiredAt;
            Status = changeStatus;
        }

        public static UserPendingChange Restore(
            UserPendingChangeId userPendingChangeId,
            UserId userId,
            ConfirmationToken confirmationToken,
            UserChange userChange,
            DateTimeOffset expiresAt,
            DateTimeOffset? confirmationTime,
            DateTimeOffset? expiredAt,
            ChangeStatus changeStatus,
            DateTimeOffset createdAt)
        {
            DomainGuard.AgainstNull<UserPendingChange>(
                (userPendingChangeId, nameof(userPendingChangeId)),
                (userId, nameof(userId)),
                (confirmationToken, nameof(confirmationToken)),
                (userChange, nameof(userChange))
            );

            ValidateState(userPendingChangeId, changeStatus, expiresAt, confirmationTime, expiredAt, createdAt);

            return new UserPendingChange(userPendingChangeId, userId, confirmationToken, userChange, expiresAt, confirmationTime, expiredAt, changeStatus, createdAt);
        }
        #endregion

        #region Behavior
        internal Result ConfirmChange(string confirmationToken, DateTimeOffset now)
        {
            if (Status == ChangeStatus.Confirmed)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<UserPendingChange>(nameof(Status)));

            if (Status == ChangeStatus.Expired)
                return Result.Failure(ChangeErrors.Expired<UserPendingChange>());

            if (ExpiresAt <= now)
            {
                Status = ChangeStatus.Expired;
                ExpiredAt = now;

                return Result.Failure(ChangeErrors.Expired<UserPendingChange>());
            }

            if (ConfirmationToken.Value != confirmationToken)
                return Result.Failure(ChangeErrors.InvalidConfirmationToken<UserPendingChange>());

            Status = ChangeStatus.Confirmed;
            ConfirmationTime = now;

            return Result.Success();
        }

        internal void MarkAsExpired(DateTimeOffset now)
        {
            if (Status == ChangeStatus.Expired)
                return;

            if (now <= ExpiresAt)
                return;

            Status = ChangeStatus.Expired;
            ExpiredAt = now;
        }
        #endregion

        #region Guard
        private static void ValidateState(
            UserPendingChangeId userPendingChangeId,
            ChangeStatus changeStatus,
            DateTimeOffset expiresAt,
            DateTimeOffset? confirmationTime,
            DateTimeOffset? expiredAt,
            DateTimeOffset createdAt)
        {
            if (!Enum.IsDefined(changeStatus))
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<UserPendingChange>(nameof(changeStatus), changeStatus.ToString());

            if (expiresAt <= createdAt)
                throw DomainInvariantViolationException.BrokenState<UserPendingChange>(
                    $"End time '{nameof(expiresAt)}' cannot be less than creation time '{nameof(createdAt)}'",
                    new Dictionary<string, object?>
                    {
                        ["expiresAt"] = expiresAt,
                        ["createdAt"] = createdAt
                    }
                );

            bool isValidState = (changeStatus, confirmationTime, expiredAt) switch
            {
                (ChangeStatus.Active, null, null) => true,
                (ChangeStatus.Confirmed, not null, null) => true,
                (ChangeStatus.Expired, null, not null) => true,
                _ => false
            };

            if (!isValidState)
            {
                throw DomainInvariantViolationException.BrokenState<UserPendingChange>(
                    $"Dates conflict. ConfirmationTime: {confirmationTime?.ToString() ?? "null"}, ExpiredAt: {expiredAt?.ToString() ?? "null"}",
                    new Dictionary<string, object?>
                    {
                        ["Id"] = userPendingChangeId,
                        ["ChangeStatus"] = changeStatus,
                        ["ConfirmationTime"] = confirmationTime,
                        ["ExpiredAt"] = expiredAt
                    }
                );
            }
        }
        #endregion
    }
}
