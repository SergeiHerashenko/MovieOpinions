using Authorization.Domain.Common.Errors.Restriction;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validations;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersRestriction.ValueObjects;

namespace Authorization.Domain.UsersRestriction
{
    public class UserRestrictionSession : AggregateRoot<UserRestrictionSessionId, Guid>
    {
        public UserId UserId { get; private set; }

        private readonly List<UserRestrictionId> _activeRestrictionIds = new();

        public IReadOnlyCollection<UserRestrictionId> ActiveRestrictions => _activeRestrictionIds;

        public int TotalBlockedDays {  get; private set; }

        #region Creation
        private UserRestrictionSession(
            UserRestrictionSessionId userRestrictionSessionId,
            UserId userId,
            List<UserRestrictionId> userRestrictionIds,
            int totalBlockedDays)
            : base(userRestrictionSessionId)
        {
            UserId = userId;
            _activeRestrictionIds.AddRange(userRestrictionIds);
            TotalBlockedDays = totalBlockedDays;
        }

        public static Result<UserRestrictionSession> Create(
            UserId userId,
            IEnumerable<UserRestriction> userRestrictions)
        {
            if (userId is null)
                return Result<UserRestrictionSession>.Failure(RestrictionError.Empty<UserRestrictionSession>(nameof(userId)));

            var restrictions = userRestrictions.ToList();

            if (!userRestrictions.Any())
                return Result<UserRestrictionSession>.Failure(RestrictionError.Empty<UserRestrictionSession>(nameof(userRestrictions)));

            var activeRestrictionsIds = restrictions
                .Select(r => r.Id)
                .ToList();

            var totalBlockedDays = userRestrictions
                .Sum(r => r.RestrictionRule.DurationDay);

            return Result<UserRestrictionSession>.Success(
                new UserRestrictionSession(
                    UserRestrictionSessionId.CreateUnique(),
                    userId,
                    activeRestrictionsIds,
                    totalBlockedDays
                )
            );
        }
        #endregion

        #region Restore
        private UserRestrictionSession(
            UserRestrictionSessionId userRestrictionSessionId,
            UserId userId,
            List<UserRestrictionId> userRestrictionIds,
            int totalBlockedDays,
            DateTimeOffset createdAt)
            : base(userRestrictionSessionId, createdAt)
        {
            UserId = userId;
            _activeRestrictionIds.AddRange(userRestrictionIds);
            TotalBlockedDays = totalBlockedDays;
        }

        public static UserRestrictionSession Restore(
            UserRestrictionSessionId userRestrictionSessionId,
            UserId userId,
            List<UserRestrictionId> userRestrictionIds,
            int totalBlockedDays,
            DateTimeOffset createdAt)
        {
            DomainGuard.AgainstNull<UserRestrictionSession>(
                (userRestrictionSessionId, nameof(userRestrictionSessionId)),
                (userId, nameof(userId)),
                (userRestrictionIds, nameof(userRestrictionIds))
            );

            if(totalBlockedDays < 0)
                throw DomainDataInconsistencyException.ValueOutOfRange<UserRestrictionSession>(nameof(totalBlockedDays), totalBlockedDays);

            return new UserRestrictionSession(userRestrictionSessionId, userId, userRestrictionIds, totalBlockedDays, createdAt);
        }
        #endregion

        #region Behavior
        public Result Refresh(IEnumerable<UserRestriction> restrictions, DateTimeOffset now)
        {
            var list = restrictions.ToList();

            if (!list.Any())
                return Result.Failure(RestrictionError.Empty<UserRestriction>(nameof(restrictions)));

            var active = list
                .Where(r => r.IsActive(now))
                .ToList();

            _activeRestrictionIds.Clear();
            _activeRestrictionIds.AddRange(active.Select(r => r.Id));

            TotalBlockedDays = active.Sum(r => r.RestrictionRule.DurationDay);

            return Result.Success();
        }
        #endregion
    }
}
