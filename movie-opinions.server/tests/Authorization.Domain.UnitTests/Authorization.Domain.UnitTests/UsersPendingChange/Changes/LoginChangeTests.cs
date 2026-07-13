using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingChange.Changes;
using Authorization.Domain.UsersPendingChange.Enums;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersPendingChange.Changes
{
    public class LoginChangeTests
    {
        private readonly Login _validLogin = Login.From(Email.Create("test@gmail.com").Value);

        #region Creation
        [Fact]
        public void Constructor_ShouldCreateLoginChange_WithCorrectData()
        {
            var change = new LoginChange(_validLogin);

            change.NewLogin.Should().Be(_validLogin);
            change.Value.Should().Be(_validLogin.Value);
            change.Type.Should().Be(UserChangeType.LoginChange);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateValidLoginChange_FromEmail()
        {
            var loginValue = "restore@test.com";
            var change = LoginChange.Restore(loginValue, LoginType.Email);

            change.NewLogin.Should().NotBeNull();
            change.NewLogin.Value.Should().Be(loginValue);
            change.Type.Should().Be(UserChangeType.LoginChange);
        }

        [Fact]
        public void Restore_ShouldCreateValidLoginChange_FromPhone()
        {
            var phoneValue = "501234567";
            var change = LoginChange.Restore(phoneValue, LoginType.Phone, "+380");

            change.NewLogin.Should().NotBeNull();
            change.NewLogin.Value.Should().Be($"+380{phoneValue}");
            change.Type.Should().Be(UserChangeType.LoginChange);
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeEqual_WhenNewLoginsAreSame()
        {
            var login = Login.From(Email.Create("test@gmail.com").Value);
            var change1 = new LoginChange(login);
            var change2 = new LoginChange(login);

            change1.Should().Be(change2);
            change1.GetHashCode().Should().Be(change2.GetHashCode());
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenNewLoginsAreDifferent()
        {
            var change1 = new LoginChange(Login.From(Email.Create("test@gmail.com").Value));
            var change2 = new LoginChange(Login.From(Email.Create("test1@gmail.com").Value));

            change1.Should().NotBe(change2);
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnNewLogin()
        {
            var change = new LoginChange(_validLogin);

            var components = change.GetEqualityComponents();

            components.Should().ContainSingle().Which.Should().Be(_validLogin);
        }
        #endregion
    }
}
