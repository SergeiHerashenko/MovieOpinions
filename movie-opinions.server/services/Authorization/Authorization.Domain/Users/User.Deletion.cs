using Authorization.Domain.Common.Errors.Common;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Deletion;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        private Result Delete(DateTimeOffset now, DeletionReason reason)
        {
            if (IsDeleted)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(IsDeleted)));

            var createDeletionResult = UserDeletion.Create(Id, Login, now, reason);

            if (createDeletionResult.IsFailure)
                return createDeletionResult;

            var deletion = createDeletionResult.Value;

            _deletion = deletion;

            AddAggregateChange(new UserDeletionCreated(deletion, now));

            return Result.Success();
        }

        public Result<UserDeletion> GetDeletion()
        {
            if (_deletion is null)
                return Result<UserDeletion>.Failure(DeletionErrors.NotDeleteUser<User>());

            return Result<UserDeletion>.Success(_deletion);
        }

        public Result Undelete(DateTimeOffset now)
        {
            if (_deletion is null)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<User>(nameof(IsDeleted)));

            var result = _deletion.Undelete(now);

            if (result.IsFailure)
                return result;

            AddDomainEvent(new UserUndeletedEvent(_deletion.Id, Login, now));

            AddAggregateChange(new UserDeletionUpdated(_deletion, now));

            return Result.Success();
        }

        public bool UpdateExpirationStatus(DateTimeOffset now)
        {
            if (_deletion is null)
                return false;

            if (!_deletion.MarkAsExpired(now))
                return false;

            AddAggregateChange(new UserDeletionUpdated(_deletion, now));

            return true;
        }
    }
}
