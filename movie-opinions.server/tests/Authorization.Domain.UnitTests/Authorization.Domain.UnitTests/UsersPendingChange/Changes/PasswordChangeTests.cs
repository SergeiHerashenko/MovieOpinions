using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersPendingChange.Changes;
using Authorization.Domain.UsersPendingChange.Enums;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersPendingChange.Changes
{
    public class PasswordChangeTests
    {
        private readonly Password _validPassword = Password.Create("StrongPass123!").Value;

        #region Creation
        [Fact]
        public void Constructor_ShouldCreatePasswordChange_WithCorrectData()
        {
            var change = new PasswordChange(_validPassword);

            change.NewPassword.Should().Be(_validPassword);
            change.Value.Should().Be(_validPassword.Value);
            change.Type.Should().Be(UserChangeType.PasswordChange);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateValidPasswordChange()
        {
            var passwordHash = "$2a$11$examplehashedpassword1234567890";
            var change = PasswordChange.Restore(passwordHash);

            change.NewPassword.Should().NotBeNull();
            change.NewPassword.Value.Should().Be(passwordHash);
            change.Type.Should().Be(UserChangeType.PasswordChange);
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeEqual_WhenPasswordsAreSame()
        {
            var password = Password.Create("SameStrongPass456!").Value;
            var change1 = new PasswordChange(password);
            var change2 = new PasswordChange(password);

            change1.Should().Be(change2);
            change1.GetHashCode().Should().Be(change2.GetHashCode());
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenPasswordsAreDifferent()
        {
            var change1 = new PasswordChange(Password.Create("PasswordOne123!").Value);
            var change2 = new PasswordChange(Password.Create("PasswordTwo456!").Value);

            change1.Should().NotBe(change2);
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnNewPassword()
        {
            var change = new PasswordChange(_validPassword);

            var components = change.GetEqualityComponents();

            components.Should().ContainSingle().Which.Should().Be(_validPassword);
        }
        #endregion
    }
}
