using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Rules.DomainPart;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects.EmailUser.Rules.DomainPart
{
    public class TooLongDomainPartRuleTests
    {
        private readonly TooLongDomainPartRule _systemUnderTest = new();
        private const int MaxLength = 40;

        [Fact]
        public void Validate_WhenLengthEqualsMax_ShouldReturnNull()
        {
            // Arrange
            var value = new string('a', MaxLength);

            // Act
            var result = _systemUnderTest.Validate(value);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Validate_WhenLengthLessThanMax_ShouldReturnNull()
        {
            // Arrange
            var value = new string('a', MaxLength - 1);

            // Act
            var result = _systemUnderTest.Validate(value);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("gmail.com")]
        [InlineData("x.co")]
        public void Validate_WhenNormalDomain_ShouldReturnNull(string value)
        {
            // Act
            var result = _systemUnderTest.Validate(value);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Validate_WhenLengthGreaterThanMax_ShouldReturnFailure()
        {
            // Arrange
            var value = new string('a', MaxLength + 1);

            // Act
            var result = _systemUnderTest.Validate(value);

            // Assert
            result.Should().NotBeNull();

            result!.Error.Should().NotBeNull();
            result.Error.Code.Should().Be(DomainErrorCodes.Email.TooLongEmailDomainPart);
            result.Error.ErrorType.Should().Be(ErrorType.OutOfRange);

            result.BuildException.Should().NotBeNull();
            var exception = result.BuildException!.Invoke();
            exception.Should().BeOfType<DomainDataInconsistencyException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WhenEmptyOrWhitespace_ShouldReturnNull(string? value)
        {
            // Act
            var result = _systemUnderTest.Validate(value!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Priority_ShouldBeLength()
        {
            _systemUnderTest.Priority.Should().Be(ValidationPriority.Length);
        }
    }
}
