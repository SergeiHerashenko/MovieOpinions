using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects.LoginUser
{
    public class EmailLoginTests
    {
        #region Creation
        [Fact]
        public void Should_ReturnCorrectValueAndType()
        {
            var email = Email.Create("test@example.com").Value;
            var login = new EmailLogin(email);

            login.Value.Should().Be("test@example.com");
            login.Type.Should().Be(LoginType.Email);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateValidObject()
        {
            string emailValue = "restore@test.com";

            var login = EmailLogin.Restore(emailValue);

            login.Email.Value.Should().Be(emailValue);
            login.Type.Should().Be(LoginType.Email);
        }
        #endregion

        #region Equality
        [Fact]
        public void Should_BeEqual_WhenEmailsAreSame()
        {
            var email = Email.Create("same@test.com").Value;
            var login1 = new EmailLogin(email);
            var login2 = new EmailLogin(email);

            login1.Should().Be(login2);
            login1.GetHashCode().Should().Be(login2.GetHashCode());
        }

        [Fact]
        public void Should_NotBeEqual_WhenEmailsAreDifferent()
        {
            var login1 = new EmailLogin(Email.Create("a@test.com").Value);
            var login2 = new EmailLogin(Email.Create("b@test.com").Value);

            login1.Should().NotBe(login2);
        }
        #endregion
    }
}
