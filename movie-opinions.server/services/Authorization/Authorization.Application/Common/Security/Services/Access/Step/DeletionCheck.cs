using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Security.Access;
using Authorization.Application.Common.Errors.Users;
using Authorization.Domain.Results;
using Authorization.Domain.Users;
using Authorization.Domain.Users.Enums;

namespace Authorization.Application.Common.Security.Services.Access.Step
{
    public class DeletionCheck<TMarker> : IAccessStep<TMarker>
    {
        private readonly IClock _clock;

        public DeletionCheck(IClock clock)
        {
            _clock = clock;
        }

        public int Priority => 2;

        public Task<Result> ExecuteAsync(User user, CancellationToken cancellationToken = default)
        {
            if (!user.IsDeleted)
                return Task.FromResult(Result.Success());

            user.UpdateExpirationStatus(_clock.UtcNow);

            var deletion = user.GetDeletion();

            if (deletion.Value.Status == DeletionStatus.PermanentlyDeleted)
                return Task.FromResult(Result.Failure(UserErrors.UserIsPermanentlyDeleted<DeletionCheck<TMarker>>()));

            return Task.FromResult(Result.Failure(UserErrors.UserIsDeleted<DeletionCheck<TMarker>>(deletion.Value.RestoreUntil)));
        }
    }
}
