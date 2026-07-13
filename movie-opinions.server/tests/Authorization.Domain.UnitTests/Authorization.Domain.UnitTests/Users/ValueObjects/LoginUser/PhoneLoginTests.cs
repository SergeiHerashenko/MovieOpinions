using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects.LoginUser
{
    public class PhoneLoginTests
    {
        #region Creation
        [Fact]
        public void Should_ReturnCorrectValueAndType()
        {
            var phone = Phone.Create(CountryCode.Create("+380").Value, "835644339");
            var login = new PhoneLogin(phone.Value);

            login.Value.Should().Be("+380835644339");
            login.Type.Should().Be(LoginType.Phone);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateValidObject()
        {
            string phoneValue = "+38058745669";

            var login = PhoneLogin.Restore("+380", phoneValue);

            login.Phone.Value.Should().Be(phoneValue);
            login.Type.Should().Be(LoginType.Phone);
        }
        #endregion

        #region Equality
        [Fact]
        public void Should_BeEqual_WhenPhonesAreSame()
        {
            var phone = Phone.Create(CountryCode.Create("+380").Value, "256897441").Value;
            var login1 = new PhoneLogin(phone);
            var login2 = new PhoneLogin(phone);

            login1.Should().Be(login2);
            login1.GetHashCode().Should().Be(login2.GetHashCode());
        }

        [Fact]
        public void Should_NotBeEqual_WhenPhonesAreDifferent()
        {
            var login1 = new PhoneLogin(Phone.Create(CountryCode.Create("+380").Value, "895623114").Value);
            var login2 = new PhoneLogin(Phone.Create(CountryCode.Create("+380").Value, "895623115").Value);

            login1.Should().NotBe(login2);
        }
        #endregion
    }
}
