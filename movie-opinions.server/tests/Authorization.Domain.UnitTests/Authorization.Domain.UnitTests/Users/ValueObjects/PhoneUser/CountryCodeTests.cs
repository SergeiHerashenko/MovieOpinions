using Authorization.Domain.Users.ValueObjects.PhoneUser;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects.PhoneUser
{
    public class CountryCodeTests
    {
        #region Create
        [Theory]
        [InlineData("", "EMPTY_COUNTRY_CODE")]
        [InlineData("380", "INVALID_FORMAT_COUNTRY_CODE")]
        [InlineData("+3", "INVALID_FORMAT_COUNTRY_CODE")]
        [InlineData("+12345", "INVALID_FORMAT_COUNTRY_CODE")]
        public void Create_ShouldReturnFailure_WhenValueIsInvalid(string rawCountryCode, string expectedErrorCode)
        {
            var conutryCode = CountryCode.Create(rawCountryCode);

            conutryCode.IsSuccess.Should().BeFalse();
            conutryCode.IsFailure.Should().BeTrue();
            conutryCode.Errors.Should().Contain(e => e.Code == expectedErrorCode);
        }

        [Theory]
        [InlineData("+38")]
        [InlineData("+380")]
        [InlineData("+1234")]
        public void Create_ShouldReturnSuccess_WhenValueIsValid(string rawCountryCode)
        {
            var result = CountryCode.Create(rawCountryCode);
            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be(rawCountryCode);
        }

        [Fact]
        public void Create_ShouldTrimValue_WhenValueHasSpaces()
        {
            var conutryCode = CountryCode.Create("    +380       ").Value;
            conutryCode.Value.Should().Be("+380");
        }
        #endregion
    }
}
