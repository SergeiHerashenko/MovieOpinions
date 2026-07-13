using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersRestriction;
using Authorization.Domain.UsersRestriction.Enums;
using Authorization.Domain.UsersRestriction.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersRestriction
{
    public class UserRestrictionSessionTests
    {
        private readonly UserId _validUserId = UserId.CreateUnique();
        private readonly RestrictionRule _rule30 = RestrictionRule.Create("Suspicious", 30).Value;
        private readonly RestrictionRule _rule7 = RestrictionRule.Create("Warning", 7).Value;
        private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

        #region Creation
        [Fact]
        public void Create_ShouldReturnSuccess_WhenRestrictionsAreProvided()
        {
            var restrictions = new List<UserRestriction>
            {
                CreateRestriction(_rule30),
                CreateRestriction(_rule7)
            };

            var result = UserRestrictionSession.Create(_validUserId, restrictions);

            result.IsSuccess.Should().BeTrue();
            var session = result.Value;

            session.UserId.Should().Be(_validUserId);
            session.ActiveRestrictions.Should().HaveCount(2);
            session.TotalBlockedDays.Should().Be(37);
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenUserIdIsNull()
        {
            var result = UserRestrictionSession.Create(null!, new List<UserRestriction> { CreateRestriction(_rule30) });

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "EMPTY_VALUE");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenRestrictionsCollectionIsEmpty()
        {
            var result = UserRestrictionSession.Create(_validUserId, new List<UserRestriction>());

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "EMPTY_VALUE");
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateSession_WithValidData()
        {
            var id = UserRestrictionSessionId.CreateUnique();
            var restrictionIds = new List<UserRestrictionId> { UserRestrictionId.CreateUnique() };
            var createdAt = _now.AddDays(-2);

            var session = UserRestrictionSession.Restore(id, _validUserId, restrictionIds, 45, createdAt);

            session.Id.Should().Be(id);
            session.UserId.Should().Be(_validUserId);
            session.ActiveRestrictions.Should().HaveCount(1);
            session.TotalBlockedDays.Should().Be(45);
        }

        [Fact]
        public void Restore_ShouldThrowException_WhenTotalBlockedDaysIsNegative()
        {
            var id = UserRestrictionSessionId.CreateUnique();
            var restrictionIds = new List<UserRestrictionId>();

            Action act = () => UserRestrictionSession.Restore(id, _validUserId, restrictionIds, -5, _now);

            act.Should().Throw<DomainDataInconsistencyException>();
        }
        #endregion

        #region Refresh
        [Fact]
        public void Refresh_ShouldUpdateActiveRestrictions_AndRecalculateTotalDays()
        {
            var session = CreateValidSession();
            var newRestrictions = new List<UserRestriction>
            {
                CreateRestriction(_rule30, isActive: true),
                CreateRestriction(_rule7, isActive: false)
            };

            var result = session.Refresh(newRestrictions, _now);

            result.IsSuccess.Should().BeTrue();
            session.ActiveRestrictions.Should().HaveCount(1);
            session.TotalBlockedDays.Should().Be(30);
        }

        [Fact]
        public void Refresh_ShouldReturnFailure_WhenRestrictionsCollectionIsEmpty()
        {
            var session = CreateValidSession();

            var result = session.Refresh(new List<UserRestriction>(), _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "EMPTY_VALUE");
        }
        #endregion

        #region Helpers
        private UserRestriction CreateRestriction(RestrictionRule rule, bool isActive = true)
        {
            var restriction = UserRestriction.Create(_validUserId, RestrictionType.Ban, rule).Value;

            if (!isActive)
            {
                var cancelTime = restriction.CreatedAt.AddMinutes(5);
                restriction.CancelRestriction(cancelTime);
            }

            return restriction;
        }

        private UserRestrictionSession CreateValidSession()
        {
            var restrictions = new List<UserRestriction> { CreateRestriction(_rule30) };
            var result = UserRestrictionSession.Create(_validUserId, restrictions);
            return result.Value;
        }
        #endregion
    }
}
