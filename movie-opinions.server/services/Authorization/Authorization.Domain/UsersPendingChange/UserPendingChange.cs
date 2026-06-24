using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;
using Authorization.Domain.DomainEvents.UserPendingChange;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersPendingChange.Changes;
using Authorization.Domain.UsersPendingChange.Enums;
using Authorization.Domain.UsersPendingChange.ValueObjects;

namespace Authorization.Domain.UsersPendingChange
{
    public class UserPendingChange : AggregateRoot<UserPendingChangeId, Guid>
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromMinutes(30);

        public UserId UserId { get; private set; }

        public ConfirmationToken ConfirmationToken { get; private set; }

        public UserChange Change { get; private set; }

        public DateTimeOffset ExpiresAt { get; private set; }

        public DateTimeOffset? ConfirmationTime { get; private set; }

        public DateTimeOffset? ExpiredAt { get; private set; }

        public ChangeStatus Status { get; private set; }

        #region Creation
        private UserPendingChange(UserPendingChangeId userPendingChangeId, UserId userId, ConfirmationToken confirmToken, UserChange userChange)
            : base(userPendingChangeId)
        {
            UserId = userId;
            ConfirmationToken = confirmToken;
            Change = userChange;
            ExpiresAt = CreatedAt.Add(ExpirationTime);
            ConfirmationTime = null;
            ExpiredAt = null;
            Status = ChangeStatus.Active;
        }

        public static DomainResult<UserPendingChange> Create(UserId userId, UserChange userChange)
        {
            if (userId is null)
                return DomainResult<UserPendingChange>.Failure(UserError.Empty(nameof(userId), nameof(UserPendingChange)));

            if(userChange is null)
                return DomainResult<UserPendingChange>.Failure(UserError.Empty(nameof(userChange), nameof(UserPendingChange)));

            var pendingChange = new UserPendingChange(
                UserPendingChangeId.CreateUnique(), 
                userId,
                ConfirmationToken.CreateUnique(),
                userChange
            );

            pendingChange.AddDomainEvent(
                new UserPendingChangeRequestedEvent(
                    pendingChange.Id,
                    pendingChange.UserId,
                    pendingChange.ConfirmationToken,
                    pendingChange.Change,
                    pendingChange.ExpiresAt,
                    pendingChange.CreatedAt
                )
            );

            return DomainResult<UserPendingChange>.Success(pendingChange);
        }
        #endregion

        #region Restore
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
            Change = userChange;
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
            if (userPendingChangeId is null)
                throw DomainDataInconsistencyException.EmptyOnRestore<UserPendingChange>(nameof(userPendingChangeId));

            if(userId is null)
                throw DomainDataInconsistencyException.EmptyOnRestore<UserPendingChange>(nameof(userId));

            if (confirmationToken is null)
                throw DomainDataInconsistencyException.EmptyOnRestore<UserPendingChange>(nameof(confirmationToken));

            if (userChange is null)
                throw DomainDataInconsistencyException.EmptyOnRestore<UserPendingChange>(nameof(userChange));

            if (expiresAt <= createdAt)
                throw DomainDataInconsistencyException.ConsistencyViolation<UserPendingChange>(
                    new Dictionary<string, object>
                    {
                        ["expiresAt"] = expiresAt,
                        ["createdAt"] = createdAt
                    }
                );

            ValidateState(userPendingChangeId, changeStatus, confirmationTime, expiredAt);

            return new UserPendingChange(userPendingChangeId, userId, confirmationToken, userChange, expiresAt, confirmationTime, expiredAt, changeStatus, createdAt);
        }
        #endregion

        #region Behavior
        public DomainResult ConfirmChange(DateTimeOffset confirmationTime)
        {
            if (Status == ChangeStatus.Confirmed)
                return DomainResult.Failure(UserError.NoChangesDetected("Change already confirmed!"));

            if(Status == ChangeStatus.Expired)
                return DomainResult.Failure(UserError.NoChangesDetected("Change already expired!"));

            Status = ChangeStatus.Confirmed;
            ConfirmationTime = confirmationTime;

            return DomainResult.Success();
        }

        public void MarkAsExpired(DateTimeOffset now)
        {
            if (Status == ChangeStatus.Expired)
                return;

            if (now <= ExpiresAt)
                return;

            Status = ChangeStatus.Expired;
            ExpiredAt = now;
        }
        #endregion

        #region Guards
        private static void ValidateState(
            UserPendingChangeId userPendingChangeId,
            ChangeStatus changeStatus,
            DateTimeOffset? confirmationTime,
            DateTimeOffset? expiredAt)
        {
            if (!Enum.IsDefined(changeStatus))
            {
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<UserPendingChange>(
                    changeStatus.ToString()
                );
            }

            switch (changeStatus)
            {
                case ChangeStatus.Active:
                    if (confirmationTime is not null)
                        throw DomainDataInconsistencyException.InvalidValue<UserPendingChange>(
                            nameof(confirmationTime),
                            value: confirmationTime.ToString()
                        );

                    if (expiredAt is not null)
                        throw DomainDataInconsistencyException.InvalidValue<UserPendingChange>(
                            nameof(expiredAt),
                            value: expiredAt.ToString()
                        );
                    break;

                case ChangeStatus.Confirmed:
                    if (confirmationTime is null)
                        throw DomainDataInconsistencyException.InvalidValue<UserPendingChange>(
                            nameof(confirmationTime),
                            value: confirmationTime.ToString()
                        );

                    if (expiredAt is not null)
                        throw DomainDataInconsistencyException.InvalidValue<UserPendingChange>(
                            nameof(expiredAt),
                            value: expiredAt.ToString()
                        );
                    break;

                case ChangeStatus.Expired:
                    if (confirmationTime is not null)
                        throw DomainDataInconsistencyException.InvalidValue<UserPendingChange>(
                            nameof(confirmationTime),
                            value: confirmationTime.ToString()
                        );

                    if (expiredAt is null)
                        throw DomainDataInconsistencyException.InvalidValue<UserPendingChange>(
                            nameof(expiredAt)
                        );
                    break;
            }
        }
        #endregion
    }
}