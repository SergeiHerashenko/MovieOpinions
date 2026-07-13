using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingChange.Changes;
using Authorization.Domain.UsersPendingChange.Enums;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersPendingChange.Changes
{
    public class UserChangeTests
    {
        private readonly Login _validLogin = Login.From(Email.Create("test@gmail.com").Value);
        private readonly Password _validPassword = Password.Create("StrongPass123!").Value;

        #region Factory Methods
        [Fact]
        public void From_Password_ShouldReturnPasswordChange()
        {
            var change = UserChange.From(_validPassword);

            change.Should().BeOfType<PasswordChange>();
            change.Type.Should().Be(UserChangeType.PasswordChange);
            change.Value.Should().Be(_validPassword.Value);
        }

        [Fact]
        public void From_Login_ShouldReturnLoginChange()
        {
            var change = UserChange.From(_validLogin);

            change.Should().BeOfType<LoginChange>();
            change.Type.Should().Be(UserChangeType.LoginChange);
            change.Value.Should().Be(_validLogin.Value);
        }
        #endregion

        #region Polymorphic Behavior
        [Fact]
        public void UserChange_ShouldBehaveAsCorrectDerivedType()
        {
            UserChange passwordChange = UserChange.From(_validPassword);
            UserChange loginChange = UserChange.From(_validLogin);

            passwordChange.Should().BeOfType<PasswordChange>();
            loginChange.Should().BeOfType<LoginChange>();

            passwordChange.Type.Should().Be(UserChangeType.PasswordChange);
            loginChange.Type.Should().Be(UserChangeType.LoginChange);
        }
        #endregion
    }
}
