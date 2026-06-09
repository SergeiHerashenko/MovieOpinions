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

        public static Result<UserPendingChange> Create(UserId userId, UserChange userChange)
        {
            if (userId is null)
                return Result<UserPendingChange>.Failure(UserError.Empty($"{nameof(UserPendingChange)}, field {nameof(userId)}"));

            if(userChange is null)
                return Result<UserPendingChange>.Failure(UserError.Empty($"{nameof(UserPendingChange)}, field {nameof(userChange)}"));

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

            return Result<UserPendingChange>.Success(pendingChange);
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
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"{nameof(UserPendingChange)}, field {nameof(userPendingChangeId)}");

            if(userId is null)
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"{nameof(UserPendingChange)}, field {nameof(userId)}");

            if (confirmationToken is null)
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"{nameof(UserPendingChange)}, field {nameof(confirmationToken)}");

            if (userChange is null)
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"{nameof(UserPendingChange)}, field {nameof(userChange)}");

            if (expiresAt <= createdAt)
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"{nameof(UserPendingChange)}: ExpiresAt must be after CreatedAt",
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
        public Result ConfirmChange(DateTimeOffset confirmationTime)
        {
            if (Status == ChangeStatus.Confirmed)
                return Result.Failure(UserError.NoChangesDetected("Change already confirmed!"));

            if(Status == ChangeStatus.Expired)
                return Result.Failure(UserError.NoChangesDetected("Change already expired!"));

            Status = ChangeStatus.Confirmed;
            ConfirmationTime = confirmationTime;

            return Result.Success();
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
                throw DomainDataInconsistencyException.UnsupportedDiscriminator(
                    $"Unknown {nameof(ChangeStatus)} value: {changeStatus}",
                    new Dictionary<string, object>
                    {
                        ["entity"] = nameof(ChangeStatus),
                        ["operation"] = "restore",
                        ["ChangeStatus"] = changeStatus,
                    }
                );
            }

            switch (changeStatus)
            {
                case ChangeStatus.Active:
                    if (confirmationTime is not null)
                        throw DomainDataInconsistencyException.InvalidValue(
                            $"Invalid {nameof(UserPendingChange)} state: {nameof(ChangeStatus.Active)} must not contain {nameof(confirmationTime)}!",
                            new Dictionary<string, object>
                            {
                                ["entity"] = nameof(UserPendingChange),
                                ["status"] = changeStatus,
                                ["field"] = nameof(confirmationTime),
                                ["actual"] = confirmationTime,
                                ["expected"] = "null",
                                ["aggregateId"] = userPendingChangeId.Value,
                            }
                        );

                    if (expiredAt is not null)
                        throw DomainDataInconsistencyException.InvalidValue(
                            $"Invalid {nameof(UserPendingChange)} state: {nameof(ChangeStatus.Active)} must not contain {nameof(expiredAt)}.",
                            new Dictionary<string, object>
                            {
                                ["entity"] = nameof(UserPendingChange),
                                ["status"] = changeStatus,
                                ["field"] = nameof(expiredAt),
                                ["actual"] = expiredAt,
                                ["expected"] = "null",
                                ["aggregateId"] = userPendingChangeId.Value,
                            }
                        );
                    break;

                case ChangeStatus.Confirmed:
                    if (confirmationTime is null)
                        throw DomainDataInconsistencyException.InvalidValue(
                            $"Invalid {nameof(UserPendingChange)} state: {nameof(ChangeStatus.Confirmed)} requires {nameof(confirmationTime)} to be set!",
                            new Dictionary<string, object>
                            {
                                ["entity"] = nameof(UserPendingChange),
                                ["status"] = changeStatus,
                                ["field"] = nameof(confirmationTime),
                                ["expected"] = "not null",
                                ["aggregateId"] = userPendingChangeId.Value,
                            }
                        );

                    if (expiredAt is not null)
                        throw DomainDataInconsistencyException.InvalidValue(
                            $"Invalid {nameof(UserPendingChange)} state: {nameof(ChangeStatus.Confirmed)} must not contain {nameof(expiredAt)}!",
                            new Dictionary<string, object>
                            {
                                ["entity"] = nameof(UserPendingChange),
                                ["status"] = changeStatus,
                                ["field"] = nameof(expiredAt),
                                ["expected"] = "null",
                                ["aggregateId"] = userPendingChangeId.Value,
                            }
                        );
                    break;

                case ChangeStatus.Expired:
                    if (confirmationTime is not null)
                        throw DomainDataInconsistencyException.InvalidValue(
                            $"Invalid {nameof(UserPendingChange)} state: {nameof(ChangeStatus.Expired)} must not contain {nameof(confirmationTime)}!",
                            new Dictionary<string, object>
                            {
                                ["entity"] = nameof(UserPendingChange),
                                ["status"] = changeStatus,
                                ["field"] = nameof(confirmationTime),
                                ["actual"] = confirmationTime,
                                ["expected"] = "null",
                                ["aggregateId"] = userPendingChangeId.Value,
                            }
                        );

                    if (expiredAt is null)
                        throw DomainDataInconsistencyException.InvalidValue(
                            $"Invalid {nameof(UserPendingChange)} state: {nameof(ChangeStatus.Expired)} requires {nameof(expiredAt)} to be set!",
                            new Dictionary<string, object>
                            {
                                ["entity"] = nameof(UserPendingChange),
                                ["status"] = changeStatus,
                                ["field"] = nameof(expiredAt),
                                ["expected"] = "not null",
                                ["aggregateId"] = userPendingChangeId.Value,
                            }
                        );
                    break;
            }
        }
        #endregion
    }
}