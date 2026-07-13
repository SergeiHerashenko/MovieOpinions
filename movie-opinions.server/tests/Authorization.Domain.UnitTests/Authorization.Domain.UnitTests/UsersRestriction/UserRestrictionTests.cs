using Authorization.Domain.Common.Errors.Restriction;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersRestriction;
using Authorization.Domain.UsersRestriction.Enums;
using Authorization.Domain.UsersRestriction.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersRestriction
{
    public class UserRestrictionTests
    {
        private readonly UserId _validUserId = UserId.CreateUnique();
        private readonly RestrictionRule _rule30Days = RestrictionRule.Create("SuspiciousActivity", 30).Value;
        private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

        #region Creation
        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            var result = UserRestriction.Create(_validUserId, RestrictionType.Ban, _rule30Days, "Test reason");

            result.IsSuccess.Should().BeTrue();
            var restriction = result.Value;

            restriction.UserId.Should().Be(_validUserId);
            restriction.RestrictionType.Should().Be(RestrictionType.Ban);
            restriction.RestrictionRule.Should().Be(_rule30Days);
            restriction.Reason.Should().Be("Test reason");
            restriction.IsRevoked.Should().BeFalse();
            restriction.CancellationDate.Should().BeNull();
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenUserIdIsNull()
        {
            var result = UserRestriction.Create(null!, RestrictionType.Ban, _rule30Days);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "EMPTY_VALUE");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenRestrictionRuleIsNull()
        {
            var result = UserRestriction.Create(_validUserId, RestrictionType.Ban, null!);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "EMPTY_VALUE");
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateValidInstance_WhenStateIsConsistent()
        {
            var id = UserRestrictionId.CreateUnique();
            var createdAt = _now.AddDays(-5);

            var restriction = UserRestriction.Restore(
                id, _validUserId, RestrictionType.Ban, _rule30Days,
                "Reason", false, createdAt, null);

            restriction.Id.Should().Be(id);
            restriction.IsRevoked.Should().BeFalse();
            restriction.CancellationDate.Should().BeNull();
        }

        [Fact]
        public void Restore_ShouldThrowException_WhenStateIsInconsistent_RevokedWithoutDate()
        {
            var id = UserRestrictionId.CreateUnique();

            Action act = () => UserRestriction.Restore(
                id, _validUserId, RestrictionType.Ban, _rule30Days,
                null, true, _now, null);

            act.Should().Throw<DomainInvariantViolationException>();
        }

        [Fact]
        public void Restore_ShouldThrowException_WhenStateIsInconsistent_NotRevokedButHasDate()
        {
            var id = UserRestrictionId.CreateUnique();

            Action act = () => UserRestriction.Restore(
                id, _validUserId, RestrictionType.Ban, _rule30Days,
                null, false, _now, _now);

            act.Should().Throw<DomainInvariantViolationException>();
        }
        #endregion

        #region Behavior
        [Fact]
        public void CancelRestriction_ShouldReturnSuccess_AndMarkAsRevoked()
        {
            var restriction = CreateValidRestriction();
            var cancelTime = _now.AddHours(3);

            var result = restriction.CancelRestriction(cancelTime);

            result.IsSuccess.Should().BeTrue();
            restriction.IsRevoked.Should().BeTrue();
            restriction.CancellationDate.Should().Be(cancelTime);
        }

        [Fact]
        public void CancelRestriction_ShouldReturnSuccess_IfAlreadyRevoked()
        {
            var restriction = CreateValidRestriction();
            restriction.CancelRestriction(_now);

            var result = restriction.CancelRestriction(_now.AddHours(1));

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void GetExpirationDate_ShouldReturnCorrectDate()
        {
            var restriction = CreateValidRestriction();

            var expiration = restriction.GetExpirationDate();

            expiration.Should().BeCloseTo(restriction.CreatedAt.AddDays(30), TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void IsActive_ShouldReturnTrue_WhenNotRevokedAndNotExpired()
        {
            var restriction = CreateValidRestriction();

            restriction.IsActive(_now.AddDays(10)).Should().BeTrue();
        }

        [Fact]
        public void IsActive_ShouldReturnFalse_WhenRevoked()
        {
            var restriction = CreateValidRestriction();

            var cancelTime = restriction.CreatedAt.AddMinutes(5);
            var result = restriction.CancelRestriction(cancelTime);
            result.IsSuccess.Should().BeTrue("CancelRestriction should succeed");
            restriction.IsRevoked.Should().BeTrue();

            restriction.IsActive(_now.AddDays(10)).Should().BeFalse();
        }

        [Fact]
        public void GetRemainingTime_ShouldReturnCorrectDuration()
        {
            var restriction = CreateValidRestriction();
            var currentTime = restriction.CreatedAt.AddDays(10);

            var remaining = restriction.GetRemainingTime(currentTime);

            remaining.Should().BeCloseTo(TimeSpan.FromDays(20), TimeSpan.FromSeconds(10));
        }
        #endregion

        #region Helpers
        private UserRestriction CreateValidRestriction()
        {
            var result = UserRestriction.Create(_validUserId, RestrictionType.Ban, _rule30Days, "Test");
            return result.Value;
        }
        #endregion
    }
}
