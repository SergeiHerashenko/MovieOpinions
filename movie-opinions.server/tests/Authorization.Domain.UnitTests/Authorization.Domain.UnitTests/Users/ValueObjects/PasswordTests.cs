using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects
{
    public class PasswordTests
    {
        #region Creation
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldReturnFailure_WhenValueIsNullOrWhiteSpace(string? invalidHash)
        {
            var result = Password.Create(invalidHash!);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "PASSWORD_EMPTY");
        }

        [Fact]
        public void Create_ShouldReturnSuccess_WhenHashIsValid()
        {
            var validHash = "some_secure_hash_string";
            var result = Password.Create(validHash);

            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be(validHash);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldThrowException_WhenValueIsEmpty()
        {
            Action act = () => Password.Restore("");

            act.Should().Throw<DomainDataInconsistencyException>();
        }

        [Fact]
        public void Restore_ShouldReturnInstance_WhenValueIsValid()
        {
            var hash = "valid_hash";
            var password = Password.Restore(hash);

            password.Value.Should().Be(hash);
        }
        #endregion

        #region Password
        [Fact]
        public void Password_ShouldBeEqual_WhenHashesAreSame()
        {
            var hash = "secret_hash";
            var pass1 = Password.Restore(hash);
            var pass2 = Password.Restore(hash);

            pass1.Should().Be(pass2);
        }
        #endregion
    }
}
