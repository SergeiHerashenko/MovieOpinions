using Authorization.Domain.Common.Errors.Restriction;
using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersRestriction.Enums;
using Authorization.Domain.UsersRestriction.ValueObjects;

namespace Authorization.Domain.UsersRestriction
{
    public class UserRestriction : AggregateRoot<UserRestrictionId, Guid>
    {
        public UserId UserId { get; private set; }

        public RestrictionType RestrictionType { get; private set; }

        public RestrictionRule RestrictionRule { get; private set; }

        public bool IsRevoked { get; private set; }

        public DateTimeOffset? CancellationDate { get; private set; }

        #region Creation
        private UserRestriction(
            UserRestrictionId userRestrictionId, 
            UserId userId, 
            RestrictionType restrictionType,
            RestrictionRule restrictionRule)
            : base(userRestrictionId)
        {
            UserId = userId;
            RestrictionType = restrictionType;
            RestrictionRule = restrictionRule;
            IsRevoked = false;
            CancellationDate = null;
        }

        public static DomainResult<UserRestriction> Create(UserId userId, RestrictionType restrictionType, RestrictionRule restrictionRule)
        {
            if (userId is null)
                return DomainResult<UserRestriction>.Failure(RestrictionError.Empty(nameof(userId), nameof(UserRestriction)));

            if(restrictionRule is null)
                return DomainResult<UserRestriction>.Failure(RestrictionError.Empty(nameof(restrictionRule), nameof(UserRestriction)));

            return DomainResult<UserRestriction>.Success(new UserRestriction(UserRestrictionId.CreateUnique(), userId, restrictionType, restrictionRule));
        }
        #endregion

        #region Restore
        private UserRestriction(
            UserRestrictionId userRestrictionId,
            UserId userId,
            RestrictionType restrictionType,
            RestrictionRule restrictionRule,
            bool isRevoked,
            DateTimeOffset createdAt,
            DateTimeOffset? cancellationDate)
            : base(userRestrictionId, createdAt)
        {
            UserId = userId;
            RestrictionType = restrictionType;
            RestrictionRule = restrictionRule;
            IsRevoked = isRevoked;
            CancellationDate = cancellationDate;
        }

        public static UserRestriction Restore(
            UserRestrictionId userRestrictionId,
            UserId userId,
            RestrictionType restrictionType,
            RestrictionRule restrictionRule,
            bool isRevoked,
            DateTimeOffset createdAt,
            DateTimeOffset? cancellationDate)
        {
            GuardNotNull<UserRestriction>(userRestrictionId, nameof(userRestrictionId));
            GuardNotNull<UserRestriction>(userId, nameof(userId));
            GuardNotNull<UserRestriction>(restrictionRule, nameof(restrictionRule));

            if (!isRevoked && cancellationDate is not null)
                throw DomainDataInconsistencyException.ConsistencyViolation<UserRestriction>(
                    new Dictionary<string, object>
                    {
                        ["isRevoked"] = isRevoked,
                        ["cancellationDate"] = cancellationDate
                    }
                );

            if (isRevoked && cancellationDate is null)
                throw DomainDataInconsistencyException.ConsistencyViolation<UserRestriction>(
                    new Dictionary<string, object>
                    {
                        ["isRevoked"] = isRevoked
                    }
                );

            return new UserRestriction(userRestrictionId, userId, restrictionType, restrictionRule, isRevoked, createdAt, cancellationDate);
        }
        #endregion

        #region Behavior
        public DomainResult CancelRestriction(DateTimeOffset now)
        {
            if (IsRevoked)
                return DomainResult.Success();

            if (now < CreatedAt)
                return DomainResult.Failure(RestrictionError.WrongTime);

            IsRevoked = true;
            CancellationDate = now;

            return DomainResult.Success();
        }

        public DateTimeOffset GetExpirationDate()
        {
            return CreatedAt.AddDays(RestrictionRule.DurationDay);
        }

        public bool IsActive(DateTimeOffset now)
        {
            return !IsRevoked && now < GetExpirationDate();
        }

        public TimeSpan GetRemainingTime(DateTimeOffset now)
        {
            var expiration = GetExpirationDate();

            if (IsRevoked || now >= expiration)
                return TimeSpan.Zero;

            return expiration - now;
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
