using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.Policies;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        public Result RecordFailedPasswordAttempt(DateTimeOffset now)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            FailedPasswordAttempts++;
            UpdatedAt = now;

            if (FailedPasswordAttempts > MaxFailedAttempts)
            {
                var restrictionData = CreateFailedLoginBanData();

                if (restrictionData.IsFailure)
                    return restrictionData;

                var addResult = AddRestrictions(new[] { restrictionData.Value }, now);

                if (addResult.IsFailure)
                    return addResult;

                FailedPasswordAttempts = 0;
            }

            return Result.Success();
        }

        public Result LoginSuccess(DateTimeOffset now)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            if (!IsLoginConfirmed)
                return Result.Failure(LoginErrors.LoginIsNotConfirm<User>());

            FailedPasswordAttempts = 0;
            LastLoginAt = now;

            return Result.Success();
        }

        private Result<RestrictionData> CreateFailedLoginBanData()
        {
            var ruleResult = RestrictionRule.Create(RestrictionPolicy.FailedLoginBanName, RestrictionPolicy.FailedLoginBanDurationMinutes);

            if (ruleResult.IsFailure)
                return Result<RestrictionData>.Failure(ruleResult.Errors);

            return Result<RestrictionData>.Success(new RestrictionData
            {
                RestrictionType = RestrictionType.Ban,
                RestrictionRule = ruleResult.Value,
                Reason = RestrictionPolicy.FailedLoginBanReason,
                RestrictedBy = RestrictionPolicy.FailedLoginBanRestrictedBy
            });
        }
    }
}
