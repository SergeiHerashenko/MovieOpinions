using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersPendingChange.Changes;
using Authorization.Domain.UsersPendingChange.Enums;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersPendingChange.Changes
{
    public class PasswordResetTests
    {
        private readonly Password _validPassword = Password.Create("StrongPass123!").Value;

        #region Creation
        [Fact]
        public void Constructor_ShouldCreatePasswordReset_WithCorrectData()
        {
            var reset = new PasswordReset(_validPassword);

            reset.NewPassword.Should().Be(_validPassword);
            reset.Value.Should().Be(_validPassword.Value);
            reset.Type.Should().Be(UserChangeType.PasswordReset);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateValidPasswordReset()
        {
            var passwordHash = "$2a$11$examplehashedpassword1234567890";
            var reset = PasswordReset.Restore(passwordHash);

            reset.NewPassword.Should().NotBeNull();
            reset.NewPassword.Value.Should().Be(passwordHash);
            reset.Type.Should().Be(UserChangeType.PasswordReset);
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeEqual_WhenPasswordsAreSame()
        {
            var password = Password.Create("SameStrongPass456!").Value;
            var reset1 = new PasswordReset(password);
            var reset2 = new PasswordReset(password);

            reset1.Should().Be(reset2);
            reset1.GetHashCode().Should().Be(reset2.GetHashCode());
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenPasswordsAreDifferent()
        {
            var reset1 = new PasswordReset(Password.Create("PasswordOne123!").Value);
            var reset2 = new PasswordReset(Password.Create("PasswordTwo456!").Value);

            reset1.Should().NotBe(reset2);
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnNewPassword()
        {
            var reset = new PasswordReset(_validPassword);

            var components = reset.GetEqualityComponents();

            components.Should().ContainSingle().Which.Should().Be(_validPassword);
        }
        #endregion
    }
}
