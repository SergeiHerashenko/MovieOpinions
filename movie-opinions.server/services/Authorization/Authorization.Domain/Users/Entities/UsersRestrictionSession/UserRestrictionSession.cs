using Authorization.Domain.Common.Errors.Common;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Entities.UsersRestrictionSession
{
    public class UserRestrictionSession : Entity<UserRestrictionSessionId>
    {
        public UserId UserId { get; private set; }

        private readonly List<UserRestrictionId> _activeRestrictionIds = new();

        public IReadOnlyCollection<UserRestrictionId> ActiveRestrictions => _activeRestrictionIds;

        public RestrictionType RestrictionType { get; private set; }

        public int TotalBlockedMinutes { get; private set; }

        #region Creation
        private UserRestrictionSession(
            UserRestrictionSessionId userRestrictionSessionId,
            UserId userId,
            List<UserRestrictionId> userRestrictionIds,
            RestrictionType restrictionType,
            int totalBlockedMinutes)
            : base(userRestrictionSessionId)
        {
            UserId = userId;
            _activeRestrictionIds.AddRange(userRestrictionIds);
            RestrictionType = restrictionType;
            TotalBlockedMinutes = totalBlockedMinutes;
        }

        internal static Result<UserRestrictionSession> Create(
            UserId userId,
            IEnumerable<UserRestriction> userRestrictions
            )
        {
            if (userId is null)
                return Result<UserRestrictionSession>.Failure(CommonErrors.Identifier.EmptyIdentifier<UserRestrictionSession>(nameof(userId)));

            var restrictions = userRestrictions.ToList();

            if (restrictions.Count == 0)
                return Result<UserRestrictionSession>.Failure(RestrictionErrors.EmptyRestrictionList<UserRestrictionSession>());

            var restrictionType = restrictions[0].RestrictionType;

            var activeRestrictionsIds = restrictions
                .Select(r => r.Id)
                .ToList();

            var totalBlockedMinutes = restrictions
                .Sum(r => r.RestrictionRule.DurationMinute);

            return Result<UserRestrictionSession>.Success(
                new UserRestrictionSession(
                    UserRestrictionSessionId.Create(),
                    userId,
                    activeRestrictionsIds,
                    restrictionType,
                    totalBlockedMinutes
                )
            );
        }
        #endregion

        #region Restoration
        private UserRestrictionSession(
            UserRestrictionSessionId userRestrictionSessionId,
            UserId userId,
            List<UserRestrictionId> userRestrictionIds,
            RestrictionType restrictionType,
            int totalBlockedMinutes,
            DateTimeOffset createdAt)
            : base(userRestrictionSessionId, createdAt)
        {
            UserId = userId;
            _activeRestrictionIds.AddRange(userRestrictionIds);
            RestrictionType = restrictionType;
            TotalBlockedMinutes = totalBlockedMinutes;
        }

        public static UserRestrictionSession Restore(
            UserRestrictionSessionId userRestrictionSessionId,
            UserId userId,
            List<UserRestrictionId> userRestrictionIds,
            RestrictionType restrictionType,
            int totalBlockedMinutes,
            DateTimeOffset createdAt)

        {
            DomainGuard.AgainstNull<UserRestrictionSession>(
                (userRestrictionSessionId, nameof(userRestrictionSessionId)),
                (userId, nameof(userId))
            );

            if (!userRestrictionIds.Any())
                throw DomainDataInconsistencyException.Empty<UserRestrictionSession>(nameof(userRestrictionIds));

            if (!Enum.IsDefined(typeof(RestrictionType), restrictionType))
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<UserRestrictionSession>(nameof(restrictionType), restrictionType.ToString());

            if (totalBlockedMinutes < 0)
                throw DomainDataInconsistencyException.ValueOutOfRange<UserRestrictionSession>(nameof(totalBlockedMinutes), totalBlockedMinutes);

            return new UserRestrictionSession(userRestrictionSessionId, userId, userRestrictionIds, restrictionType, totalBlockedMinutes, createdAt);
        }
        #endregion

        #region Behavior
        internal Result AddRestrictions(IEnumerable<UserRestriction> userRestrictions)
        {
            var restrictions = userRestrictions
                .Where(x => !_activeRestrictionIds.Contains(x.Id))
                .ToList();

            if (restrictions.Count == 0)
                return Result.Success();

            foreach (var restriction in restrictions)
            {
                if (restriction.RestrictionType != RestrictionType)
                    return Result.Failure(RestrictionErrors.InvalidRestrictionType<UserRestrictionSession>(restriction.RestrictionType.ToString()));

                if(restriction.UserId != UserId)
                    return Result.Failure(CommonErrors.Identifier.IdentifierMismatch<UserRestrictionSession>(nameof(UserId)));
            }

            var totalMinutes = 0;

            foreach (var restriction in restrictions)
            {
                _activeRestrictionIds.Add(restriction.Id);

                totalMinutes += restriction.RestrictionRule.DurationMinute;
            }

            TotalBlockedMinutes += totalMinutes;

            return Result.Success();
        }

        internal Result RemoveRestriction(UserRestriction userRestriction)
        {
            if (userRestriction is null)
                return Result.Failure(RestrictionErrors.EmptyRestriction<UserRestrictionSession>());

            if (userRestriction.UserId != UserId)
                return Result.Failure(CommonErrors.Identifier.IdentifierMismatch<UserRestrictionSession>(nameof(UserId)));

            if (userRestriction.RestrictionType != RestrictionType)
                return Result.Failure(RestrictionErrors.InvalidRestrictionType<UserRestrictionSession>(userRestriction.RestrictionType.ToString()));

            if (!_activeRestrictionIds.Contains(userRestriction.Id))
                return Result.Failure(RestrictionErrors.NotFoundRestriction<UserRestrictionSession>());

            if(_activeRestrictionIds.Count == 1)
            {
                _activeRestrictionIds.Clear();
                TotalBlockedMinutes = 0;

                return Result.Success();
            }

            _activeRestrictionIds.Remove(userRestriction.Id);
            TotalBlockedMinutes -= userRestriction.RestrictionRule.DurationMinute;

            if (TotalBlockedMinutes < 0)
                throw DomainInvariantViolationException.BrokenState<UserRestrictionSession>(
                    "Total blocked minutes became negative after removing a restriction.",
                    new Dictionary<string, object?>
                    {
                        ["TotalBlockedMinutes"] = TotalBlockedMinutes,
                        ["RestrictionId"] = userRestriction.Id.Value
                    }
                );

            return Result.Success();
        }

        internal bool ContainsRestriction(UserRestrictionId restrictionId)
        {
            return _activeRestrictionIds.Contains(restrictionId);
        }

        internal bool IsEmpty()
            => _activeRestrictionIds.Count == 0;

        public DateTimeOffset GetExpirationDate()
        {
            return CreatedAt.AddMinutes(TotalBlockedMinutes);
        }

        public bool IsActive(DateTimeOffset now)
        {
            return now < GetExpirationDate();
        }

        public TimeSpan GetRemainingTime(DateTimeOffset now)
        {
            var expiration = GetExpirationDate();

            if (now >= expiration)
                return TimeSpan.Zero;

            return expiration - now;
        }
        #endregion
    }
}
