using Authorization.Application.Common.ApplicationErrors.Users;
using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Security.Access;
using Authorization.Domain.Results;
using Authorization.Domain.Users;
using Authorization.Domain.UsersRestriction;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Features.Authentication.SignIn.Steps
{
    public class BlockCheck : IAccessStep
    {
        private readonly IUserRestrictionSessionRepository _userRestrictionSessionRepository;
        private readonly IUserRestrictionRepository _userRestrictionRepository;

        private readonly IClock _clock;

        private readonly ILogger<BlockCheck> _logger;

        public BlockCheck(
            IUserRestrictionSessionRepository userRestrictionSessionRepository,
            IUserRestrictionRepository userRestrictionRepository,
            IClock clock,
            ILogger<BlockCheck> logger)
        {
            _userRestrictionSessionRepository = userRestrictionSessionRepository;
            _userRestrictionRepository = userRestrictionRepository;
            _clock = clock;
            _logger = logger;
        }

        public int Priority => 1;

        public async Task<Result> ExecuteAsync(User user)
        {
            if (!user.IsBlocked)
                return Result.Success();

            var restrictionSession = await _userRestrictionSessionRepository.GetSessionRestrictionByLoginAsync(user.Login);

            if(restrictionSession is null)
            {
                _logger.LogWarning(
                    "User {UserId} is marked as blocked, but restriction session was not found.",
                    user.Id.Value
                );

                user.RemoveBlock(_clock.UtcNow);

                return Result.Success();
            }

            DateTimeOffset expiresAt = restrictionSession.CreatedAt.AddDays(restrictionSession.TotalBlockedDays);

            if (_clock.UtcNow > expiresAt)
            {
                await RestoreAccessAsync(user, restrictionSession);

                return Result.Success();
            }

            var restrictions = await _userRestrictionRepository.GetRestrictionsAsync(restrictionSession.ActiveRestrictions);

            if (restrictions is null)
            {
                _logger.LogError("Restriction session {SessionId} contains active restrictions, but no restriction records were found.",
                    restrictionSession.Id.Value
                );

                await RestoreAccessAsync(user, restrictionSession);

                return Result.Success();
            }

            var reasons = restrictions
                .Select(x => $"{x.RestrictionRule.Name} ({x.RestrictionRule.DurationDays} днів)")
                .ToList();

            return Result.Failure(UserErrors.ActiveRestrictions(reasons, expiresAt));
        }

        private async Task RestoreAccessAsync(User user, UserRestrictionSession session)
        {
            await _userRestrictionSessionRepository.DeleteAsync(session.Id);

            user.RemoveBlock(_clock.UtcNow);
        }
    }
}
