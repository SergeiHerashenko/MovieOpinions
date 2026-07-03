using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validations;
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
                return Result<UserPendingChange>.Failure(UserErrors.IdentifierError.EmptyIdentifier<UserPendingChange>());

            if(userChange is null)
                return Result<UserPendingChange>.Failure(UserErrors.IdentifierError.EmptyIdentifier<UserPendingChange>());

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
            DomainGuard.AgainstNull<UserPendingChange>(
                (userPendingChangeId, nameof(userPendingChangeId)),
                (userId, nameof(userId)),
                (confirmationToken, nameof(confirmationToken)),
                (userChange, nameof(userChange))
            );

            if (expiresAt <= createdAt)
                throw DomainInvariantViolationException.BrokenState<UserPendingChange>(
                    $"End time '{nameof(expiresAt)}' cannot be less than creation time '{nameof(createdAt)}'",
                    new Dictionary<string, object?>
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
                return Result.Failure(UserErrors.GeneralError.AlreadyConfirmed<UserPendingChange>());

            if(Status == ChangeStatus.Expired)
                return Result.Failure(UserErrors.GeneralError.OperationIsNotAllowed<UserPendingChange>("Change already expired!"));

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
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<UserPendingChange>(nameof(changeStatus), changeStatus.ToString());

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