using Authorization.Domain.Users.ValueObjects.PhoneUser;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects.PhoneUser
{
    public class PhoneTests
    {
        private static CountryCode ValidCountryCode => CountryCode.Create("+380").Value;

        #region Create
        [Theory]
        [InlineData("", "EMPTY_PHONE")]
        [InlineData("123", "TOO_SHORT_PHONE")]
        [InlineData("12312312312312312312", "TOO_LONG_PHONE")]
        public void Create_ShouldReturnFailure_WhenPhoneIsInvalid(string? rawPhone, string expectedErrorCode)
        {
            var phone = Phone.Create(ValidCountryCode, rawPhone ?? string.Empty);

            phone.IsSuccess.Should().BeFalse();
            phone.IsFailure.Should().BeTrue();
            phone.Errors.Should().Contain(e => e.Code == expectedErrorCode);
        }

        [Fact]
        public void Create_ShouldReturnSuccess_WhenAllDataIsValid()
        {
            var phone = Phone.Create(ValidCountryCode, "1234567");

            phone.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData("123-45-67", "1234567")]
        [InlineData("(123) 456 78 90", "1234567890")]
        [InlineData("  123 45 67  ", "1234567")]
        public void Create_ShouldCleanPhoneNumberCorrectly(string rawPhone, string expectedCleanedValue)
        {
            var phone = Phone.Create(ValidCountryCode, rawPhone);

            phone.Value.Value.Should().Be(expectedCleanedValue);
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenCountryCodeIsNull()
        {
            var phone = Phone.Create(null!, "1234567");

            phone.IsFailure.Should().BeTrue();
            phone.Errors.Should().Contain(e => e.Code == "EMPTY_COUNTRY_CODE");
        }
        #endregion

        #region Operators
        [Fact]
        public void GetFullNumber_ShouldReturnConcatenatedString()
        {
            var phone = Phone.Create(ValidCountryCode, "1234567").Value;

            phone.GetFullNumber().Should().Be("+3801234567");
        }
        #endregion

        #region Equality
        [Fact]
        public void Phone_ShouldBeEqual_WhenComponentsAreEqual()
        {
            var phone1 = Phone.Create(ValidCountryCode, "1234567").Value;
            var phone2 = Phone.Create(ValidCountryCode, "1234567").Value;

            phone1.Should().Be(phone2);
        }
        #endregion
    }
}
