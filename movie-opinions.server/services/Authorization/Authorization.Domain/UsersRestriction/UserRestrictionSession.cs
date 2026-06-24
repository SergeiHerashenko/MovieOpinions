using Authorization.Domain.Common.Errors.Restriction;
using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;
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

        public static DomainResult<UserRestrictionSession> Create(
            UserId userId,
            IEnumerable<UserRestriction> userRestrictions)
        {
            if (userId is null)
                return DomainResult<UserRestrictionSession>.Failure(RestrictionError.Empty(nameof(userId), nameof(UserRestrictionSession)));

            var restrictions = userRestrictions.ToList();

            if (!userRestrictions.Any())
                return DomainResult<UserRestrictionSession>.Failure(RestrictionError.Empty(nameof(userRestrictions), nameof(UserRestrictionSession)));

            var activeRestrictionsIds = restrictions
                .Select(r => r.Id)
                .ToList();

            var totalBlockedDays = userRestrictions
                .Sum(r => r.RestrictionRule.DurationDay);

            return DomainResult<UserRestrictionSession>.Success(
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
            GuardNotNull<UserRestrictionSessionId>(userRestrictionSessionId, nameof(userRestrictionSessionId));
            GuardNotNull<UserId>(userId, nameof(userId));
            GuardNotNull<List<UserRestrictionId>>(userRestrictionIds, nameof(userRestrictionIds));

            if(totalBlockedDays < 0)
                throw DomainDataInconsistencyException.InvalidValue<UserRestrictionSession>(
                    nameof(totalBlockedDays),
                    value: totalBlockedDays
                );

            return new UserRestrictionSession(userRestrictionSessionId, userId, userRestrictionIds, totalBlockedDays, createdAt);
        }
        #endregion

        #region Behavior
        public DomainResult Refresh(IEnumerable<UserRestriction> restrictions, DateTimeOffset now)
        {
            var list = restrictions.ToList();

            if (!list.Any())
                return DomainResult.Failure(RestrictionError.Empty(nameof(restrictions), nameof(UserRestrictionSession)));

            var active = list
                .Where(r => r.IsActive(now))
                .ToList();

            _activeRestrictionIds.Clear();
            _activeRestrictionIds.AddRange(active.Select(r => r.Id));

            TotalBlockedDays = active.Sum(r => r.RestrictionRule.DurationDay);

            return DomainResult.Success();
        }
        #endregion

        #region Guard
        private static void GuardNotNull<T>(object? value, string field)
            where T : class
        {
            if (value is null)
                throw DomainDataInconsistencyException.EmptyOnRestore<T>(
                    field
                );
        }
        #endregion
    }
}
