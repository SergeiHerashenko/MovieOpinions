using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.UsersRestriction.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersRestriction.ValueObjects
{
    public class RestrictionRuleTests
    {
        #region Creation
        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            var result = RestrictionRule.Create("SuspiciousActivity", 30);

            result.IsSuccess.Should().BeTrue();
            var rule = result.Value;

            rule.Name.Should().Be("SuspiciousActivity");
            rule.DurationDays.Should().Be(30);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldReturnFailure_WhenNameIsEmpty(string? invalidName)
        {
            var result = RestrictionRule.Create(invalidName!, 15);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "EMPTY_VALUE");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Create_ShouldReturnFailure_WhenDurationDaysIsNotPositive(int invalidDays)
        {
            var result = RestrictionRule.Create("TestRule", invalidDays);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "NOT_ENOUGH_DAYS");
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateRule_WhenDataIsValid()
        {
            var rule = RestrictionRule.Restore("AccountLocked", 90);

            rule.Name.Should().Be("AccountLocked");
            rule.DurationDays.Should().Be(90);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Restore_ShouldThrowException_WhenNameIsNullOrEmpty(string? invalidName)
        {
            Action act = () => RestrictionRule.Restore(invalidName!, 10);

            act.Should().Throw<DomainDataInconsistencyException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Restore_ShouldThrowException_WhenDurationDaysIsNotPositive(int invalidDays)
        {
            Action act = () => RestrictionRule.Restore("TestRule", invalidDays);

            act.Should().Throw<DomainDataInconsistencyException>();
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeEqual_WhenAllFieldsMatch()
        {
            var rule1 = RestrictionRule.Create("Spam", 7).Value;
            var rule2 = RestrictionRule.Create("Spam", 7).Value;

            rule1.Should().Be(rule2);
            rule1.GetHashCode().Should().Be(rule2.GetHashCode());
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenAnyFieldDiffers()
        {
            var rule1 = RestrictionRule.Create("Suspicious", 30).Value;
            var rule2 = RestrictionRule.Create("Suspicious", 60).Value;

            rule1.Should().NotBe(rule2);
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnNameAndDuration()
        {
            var rule = RestrictionRule.Create("BruteForce", 14).Value;

            var components = rule.GetEqualityComponents().ToList();

            components.Should().HaveCount(2);
            components.Should().Contain("BruteForce");
            components.Should().Contain(14);
        }
        #endregion
    }
}
