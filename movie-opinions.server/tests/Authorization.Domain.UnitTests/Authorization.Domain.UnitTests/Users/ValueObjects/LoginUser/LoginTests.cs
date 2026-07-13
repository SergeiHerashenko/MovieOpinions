using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects.LoginUser
{
    public class LoginTests
    {
        #region From
        [Fact]
        public void From_Email_ShouldReturnEmailLogin()
        {
            var email = Email.Create("test@test.com").Value;
            var login = Login.From(email);

            login.Should().BeOfType<EmailLogin>();
            login.Type.Should().Be(LoginType.Email);
        }

        [Fact]
        public void From_Phone_ShouldReturnPhoneLogin()
        {
            var phone = Phone.Create(CountryCode.Create("+380").Value, "1234567").Value;
            var login = Login.From(phone);

            login.Should().BeOfType<PhoneLogin>();
            login.Type.Should().Be(LoginType.Phone);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldReturnCorrectType_ForEmail()
        {
            var login = Login.Restore("test@test.com", LoginType.Email);

            login.Should().BeOfType<EmailLogin>();
            login.Value.Should().Be("test@test.com");
        }

        [Fact]
        public void Restore_ShouldThrowException_WhenTypeIsUnsupported()
        {
            Action act = () => Login.Restore("val", (LoginType)999);

            act.Should().Throw<DomainDataInconsistencyException>();
        }
        #endregion
    }
}
