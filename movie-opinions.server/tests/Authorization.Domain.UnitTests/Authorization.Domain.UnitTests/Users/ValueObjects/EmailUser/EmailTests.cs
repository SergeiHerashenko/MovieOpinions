using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects.EmailUser
{
    public class EmailTests
    {
        #region Create
        [Theory]
        [InlineData("", "EMPTY_EMAIL")]
        [InlineData(null, "EMPTY_EMAIL")]
        [InlineData("testtesttesttesttesttesttesttest@gmail.com", "TOO_LONG_EMAIL")]
        [InlineData("test @gmail.com", "INVALID_FORMAT_EMAIL")]
        [InlineData("test@gmail,com", "INVALID_FORMAT_EMAIL")]
        public void Create_ShouldReturnFailure_WhenEmailIsInvalid(string? rawEmail, string expectedErrorCode)
        {
            var email = Email.Create(rawEmail ?? string.Empty);

            email.IsSuccess.Should().BeFalse();
            email.IsFailure.Should().BeTrue();
            email.Errors.Should().Contain(e => e.Code == expectedErrorCode);
        }

        [Theory]
        [InlineData("test@gmail.com")]
        public void Create_ShouldReturnSuccess_WhenEmailIsValid(string rawEmail)
        {
            var email = Email.Create(rawEmail);

            email.IsSuccess.Should().BeTrue();
            email.IsFailure.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldNormalizeEmail_WhenEmailHasUppercase()
        {
            var email = Email.Create("TEST@GMAIL.COM").Value;
            email.Value.Should().Be("test@gmail.com");
        }

        [Fact]
        public void Create_ShouldTrimEmail_WhenEmailHasSpaces()
        {
            var email = Email.Create("  test@gmail.com  ").Value;
            email.Value.Should().Be("test@gmail.com");
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldThrowException_WhenValueIsEmpty()
        {
            Action act = () => Email.Restore("");
            act.Should().Throw<DomainDataInconsistencyException>();
        }

        [Fact]
        public void Restore_ShouldReturnSuccess_WhenValueIsNotEmpty()
        {
            var rawValue = "test@gmail.com";
            var email = Email.Restore(rawValue);

            email.Value.Should().Be("test@gmail.com");
        }
        #endregion
    }
}
