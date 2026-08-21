using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Security.Access;
using Authorization.Application.Common.Errors.Users;
using Authorization.Domain.Results;
using Authorization.Domain.Users;
using Authorization.Domain.Users.Enums;

namespace Authorization.Application.Common.Security.Services.Access.Step
{
    public class BlockCheck<TMarker> : IAccessStep<TMarker>
    {
        private readonly IClock _clock;

        public BlockCheck(IClock clock)
        {
            _clock = clock;
        }

        public int Priority => 1;

        public Task<Result> ExecuteAsync(User user, CancellationToken cancellationToken = default)
        {
            var banSession = user.RestrictionSessions
                .FirstOrDefault(x => x.RestrictionType == RestrictionType.Ban);

            if (banSession is null)
                return Task.FromResult(Result.Success());

            if (!banSession.IsActive(_clock.UtcNow))
            {
                user.RemoveRestrictionSession(banSession.RestrictionType, _clock.UtcNow);

                return Task.FromResult(Result.Success());
            }

            return Task.FromResult(Result.Failure(UserErrors.UserIsBlocked<BlockCheck<TMarker>>(banSession.GetRemainingTime(_clock.UtcNow))));
        }
    }
}
