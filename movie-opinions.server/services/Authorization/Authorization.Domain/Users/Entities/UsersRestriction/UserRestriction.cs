using Authorization.Domain.Common.Errors.Common;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Entities.UsersRestriction
{
    public class UserRestriction : Entity<UserRestrictionId>
    {
        public UserId UserId { get; private set; }

        public RestrictionRule RestrictionRule { get; private set; }

        public RestrictionType RestrictionType { get; private set; }

        public string? Reason { get; private set; }

        public string RestrictedBy { get; private set; }

        public bool IsRevoked { get; private set; }

        public DateTimeOffset? CancellationDate { get; private set; }

        #region Creation
        private UserRestriction(
            UserRestrictionId userRestrictionId,
            UserId userId,
            RestrictionRule restrictionRule,
            RestrictionType restrictionType,
            string restrictedBy,
            string? reason)
            : base(userRestrictionId)
        {
            UserId = userId;
            RestrictionRule = restrictionRule;
            RestrictionType = restrictionType;
            Reason = reason;
            RestrictedBy = restrictedBy;
            IsRevoked = false;
            CancellationDate = null;
        }

        internal static Result<UserRestriction> Create(
            UserId userId,
            RestrictionType restrictionType,
            RestrictionRule restrictionRule,
            string restrictedBy,
            string? reason = null)
        {
            if (userId is null)
                return Result<UserRestriction>.Failure(CommonErrors.Identifier.EmptyIdentifier<UserRestriction>(nameof(userId)));

            if (restrictionRule is null)
                return Result<UserRestriction>.Failure(RestrictionErrors.EmptyRestrictionRule<UserRestriction>());

            if (!Enum.IsDefined(typeof(RestrictionType), restrictionType))
                return Result<UserRestriction>.Failure(CommonErrors.Unsupported.UnsupportedType<UserRestriction>(restrictionType.ToString()));

            var createRestriction = new UserRestriction(UserRestrictionId.Create(), userId, restrictionRule, restrictionType, restrictedBy, reason);

            return Result<UserRestriction>.Success(createRestriction);
        }
        #endregion

        #region Restoration
        private UserRestriction(
            UserRestrictionId userRestrictionId,
            UserId userId,
            RestrictionRule restrictionRule,
            RestrictionType restrictionType,
            string restrictedBy,
            string? reason,
            bool isRevoked,
            DateTimeOffset createdAt,
            DateTimeOffset? cancellationDate)
            : base(userRestrictionId, createdAt)
        {
            UserId = userId;
            RestrictionType = restrictionType;
            RestrictionRule = restrictionRule;
            Reason = reason;
            RestrictedBy = restrictedBy;
            IsRevoked = isRevoked;
            CancellationDate = cancellationDate;
        }

        public static UserRestriction Restore(
            UserRestrictionId userRestrictionId,
            UserId userId,
            RestrictionRule restrictionRule,
            RestrictionType restrictionType,
            string restrictedBy,
            string? reason,
            bool isRevoked,
            DateTimeOffset createdAt,
            DateTimeOffset? cancellationDate)
        {
            DomainGuard.AgainstNull<UserRestriction>(
                (userRestrictionId, nameof(userRestrictionId)),
                (userId, nameof(userId)),
                (restrictionRule, nameof(restrictionRule))
            );

            ValidateState(restrictionType, isRevoked, cancellationDate);

            return new UserRestriction(userRestrictionId, userId, restrictionRule, restrictionType, restrictedBy, reason, isRevoked, createdAt, cancellationDate);
        }
        #endregion

        #region Behavior
        internal Result CancelRestriction(DateTimeOffset now)
        {
            if (IsRevoked)
                return Result.Failure(CommonErrors.StateConflict.NoUpdateNeeded<UserRestriction>(nameof(IsRevoked)));

            if (now < CreatedAt)
                return Result.Failure(RestrictionErrors.WrongTime<UserRestriction>());

            IsRevoked = true;
            CancellationDate = now;

            return Result.Success();
        }

        public DateTimeOffset GetExpirationDate()
        {
            return CreatedAt.AddMinutes(RestrictionRule.DurationMinute);
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
        private static void ValidateState(RestrictionType restrictionType, bool isRevoked, DateTimeOffset? cancellationDate)
        {
            if (!Enum.IsDefined(typeof(RestrictionType), restrictionType))
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<UserRestriction>(nameof(restrictionType), restrictionType.ToString());

            bool isValidState = (isRevoked, cancellationDate) switch
            {
                (false, not null) => false,
                (true, null) => false,
                _ => true
            };

            if (!isValidState)
            {
                throw DomainInvariantViolationException.BrokenState<UserRestriction>(
                    $"Inconsistent constraint state: fields '{nameof(cancellationDate)}' and '{nameof(isRevoked)}' are not consistent!",
                    new Dictionary<string, object?>
                    {
                        ["IsRevoked"] = isRevoked,
                        ["CancellationDate"] = cancellationDate
                    }
                );
            }
        }
        #endregion
    }
}
