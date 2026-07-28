using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Rules.DomainPart;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects.EmailUser.Rules.DomainPart
{
    public class NotAllowedDomainPartRuleTests
    {
        private readonly NotAllowedDomainPartRule _systemUnderTest = new();

        [Theory]
        [InlineData("mail.ru")]
        [InlineData("MAIL.RU")]
        [InlineData("Mail.Ru")]
        [InlineData("  yandex.ru  ")]
        [InlineData("bk.ru")]
        [InlineData("inbox.ru")]
        [InlineData("list.ru")]
        [InlineData("rambler.ru")]
        [InlineData("internet.ru")]
        [InlineData("xmail.ru")]
        public void Validate_WhenBannedDomain_ShouldReturnFailure(string value)
        {
            // Act
            var result = _systemUnderTest.Validate(value);

            // Assert
            result.Should().NotBeNull();

            result!.Error.Should().NotBeNull();
            result.Error.Code.Should().Be(DomainErrorCodes.Email.NotAllowedEmailDomain);
            result.Error.ErrorType.Should().Be(ErrorType.PolicyViolation);

            result.BuildException.Should().NotBeNull();
            var exception = result.BuildException!.Invoke();
            exception.Should().BeOfType<DomainDataInconsistencyException>();
        }

        [Theory]
        [InlineData("gmail.com")]
        [InlineData("outlook.com")]
        [InlineData("company.co.uk")]
        [InlineData("mail.com")]
        public void Validate_WhenAllowedDomain_ShouldReturnNull(string value)
        {
            // Act
            var result = _systemUnderTest.Validate(value);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Validate_WhenWhitespace_ShouldReturnNull(string value)
        {
            // Act
            var result = _systemUnderTest.Validate(value);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Priority_ShouldBeBusinessRule()
        {
            _systemUnderTest.Priority.Should().Be(ValidationPriority.BusinessRule);
        }
    }
}
