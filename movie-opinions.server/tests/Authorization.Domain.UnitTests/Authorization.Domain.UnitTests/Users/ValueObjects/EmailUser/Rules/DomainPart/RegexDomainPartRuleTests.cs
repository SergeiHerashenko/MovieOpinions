using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Rules.DomainPart;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects.EmailUser.Rules.DomainPart
{
    public class RegexDomainPartRuleTests
    {
        private readonly RegexDomainPartRule _systemUnderTest = new();

        [Theory]
        [InlineData("gmail.com")]
        [InlineData("mail.google.com")]
        [InlineData("company.co.uk")]
        [InlineData("sub.domain.example.org")]
        [InlineData("a-b.com")]
        [InlineData("x.co")]
        [InlineData("GMAIL.COM")]
        public void Validate_WhenValidFormat_ShouldReturnNull(string value)
        {
            // Act
            var result = _systemUnderTest.Validate(value);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("gmail")]              
        [InlineData("gmail.")]             
        [InlineData(".com")]               
        [InlineData("gmail..com")]         
        [InlineData("-gmail.com")]         
        [InlineData("gmail-.com")]         
        [InlineData("gmail.c")]            
        [InlineData("gmail.123")]          
        [InlineData("gmail.com.")]         
        [InlineData("gmail .com")]         
        public void Validate_WhenInvalidFormat_ShouldReturnFailure(string value)
        {
            // Act
            var result = _systemUnderTest.Validate(value);

            // Assert
            result.Should().NotBeNull();

            result!.Error.Should().NotBeNull();
            result.Error.Code.Should().Be(DomainErrorCodes.Email.InvalidFormatEmailDomainPart);
            result.Error.ErrorType.Should().Be(ErrorType.Validation);

            result.BuildException.Should().NotBeNull();
            var exception = result.BuildException!.Invoke();
            exception.Should().BeOfType<DomainDataInconsistencyException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Validate_WhenEmptyOrWhitespace_ShouldReturnNull(string? value)
        {
            // Act
            var result = _systemUnderTest.Validate(value!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Priority_ShouldBeFormat()
        {
            _systemUnderTest.Priority.Should().Be(ValidationPriority.Format);
        }
    }
}
