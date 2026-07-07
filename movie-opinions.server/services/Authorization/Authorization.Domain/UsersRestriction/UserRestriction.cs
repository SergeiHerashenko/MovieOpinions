using Authorization.Domain.Common.Errors.Restriction;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validations;
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

        public string? Reason { get; private set; }

        public bool IsRevoked { get; private set; }

        public DateTimeOffset? CancellationDate { get; private set; }

        #region Creation
        private UserRestriction(
            UserRestrictionId userRestrictionId, 
            UserId userId, 
            RestrictionType restrictionType,
            RestrictionRule restrictionRule,
            string? reason)
            : base(userRestrictionId)
        {
            UserId = userId;
            RestrictionType = restrictionType;
            RestrictionRule = restrictionRule;
            Reason = reason;
            IsRevoked = false;
            CancellationDate = null;
        }

        public static Result<UserRestriction> Create(UserId userId, RestrictionType restrictionType, RestrictionRule restrictionRule, string? reason = null)
        {
            if (userId is null)
                return Result<UserRestriction>.Failure(RestrictionError.Empty<UserRestriction>(nameof(userId)));

            if(restrictionRule is null)
                return Result<UserRestriction>.Failure(RestrictionError.Empty<UserRestriction>(nameof(restrictionRule)));

            return Result<UserRestriction>.Success(new UserRestriction(UserRestrictionId.CreateUnique(), userId, restrictionType, restrictionRule, reason));
        }
        #endregion

        #region Restore
        private UserRestriction(
            UserRestrictionId userRestrictionId,
            UserId userId,
            RestrictionType restrictionType,
            RestrictionRule restrictionRule,
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
            IsRevoked = isRevoked;
            CancellationDate = cancellationDate;
        }

        public static UserRestriction Restore(
            UserRestrictionId userRestrictionId,
            UserId userId,
            RestrictionType restrictionType,
            RestrictionRule restrictionRule,
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

            if (!isRevoked && cancellationDate is not null)
                throw DomainInvariantViolationException.BrokenState<UserRestriction>(
                    $"Inconsistent restriction state: '{nameof(cancellationDate)}' cannot be set if the restriction is not revoked ({nameof(isRevoked)} is false)!",
                    new Dictionary<string, object?>
                    {
                        ["isRevoked"] = isRevoked,
                        ["cancellationDate"] = cancellationDate
                    }
                );

            if (isRevoked && cancellationDate is null)
                throw DomainInvariantViolationException.BrokenState<UserRestriction>(
                    $"Inconsistent restriction state: A revoked restriction ({nameof(isRevoked)} is true) must have a valid '{nameof(cancellationDate)}'!",
                    new Dictionary<string, object?>
                    {
                        ["isRevoked"] = isRevoked,
                        ["cancellationDate"] = cancellationDate
                    }
                );

            return new UserRestriction(userRestrictionId, userId, restrictionType, restrictionRule, reason, isRevoked, createdAt, cancellationDate);
        }
        #endregion

        #region Behavior
        public Result CancelRestriction(DateTimeOffset now)
        {
            if (IsRevoked)
                return Result.Success();

            if (now < CreatedAt)
                return Result.Failure(RestrictionError.WrongTime<UserRestriction>());

            IsRevoked = true;
            CancellationDate = now;

            return Result.Success();
        }

        public DateTimeOffset GetExpirationDate()
        {
            return CreatedAt.AddDays(RestrictionRule.DurationDays);
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
    }
}
