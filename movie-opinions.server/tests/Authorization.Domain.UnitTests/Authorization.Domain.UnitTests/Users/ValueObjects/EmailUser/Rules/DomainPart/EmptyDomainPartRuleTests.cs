using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Rules.DomainPart;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects.EmailUser.Rules.DomainPart
{
    public class EmptyDomainPartRuleTests
    {
        private readonly EmptyDomainPartRule _systemUnderTest = new();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Validate_WhenEmptyOrWhitespace_ShouldReturnFailure(string? value)
        {
            // Act
            var result = _systemUnderTest.Validate(value!);

            // Assert
            result.Should().NotBeNull();

            result!.Error.Should().NotBeNull();
            result.Error.Code.Should().Be(DomainErrorCodes.Email.EmptyEmailDomain);
            result.Error.ErrorType.Should().Be(ErrorType.EmptyValue);

            result.BuildException.Should().NotBeNull();

            var exception = result.BuildException!();
            exception.Should().BeOfType<DomainDataInconsistencyException>();
        }

        [Theory]
        [InlineData("gmail.com")]
        [InlineData("a.co")]
        [InlineData("x")]
        [InlineData(" domain.com ")]
        public void Validate_WhenHasValue_ShouldReturnNull(string value)
        {
            // Act
            var result = _systemUnderTest.Validate(value);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Priority_ShouldBePresence()
        {
            _systemUnderTest.Priority.Should().Be(ValidationPriority.Presence);
        }
    }
}
