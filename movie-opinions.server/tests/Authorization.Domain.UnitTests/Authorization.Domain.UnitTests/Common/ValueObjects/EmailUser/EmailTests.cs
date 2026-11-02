using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using System.Globalization;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.EmailUser
{
    public class EmailTests
    {
        private static string BuildEmailOfLength(int length)
        {
            var domainLength = length - 51;
            var domain =
                new string('a', 63) + "." +
                new string('b', 63) + "." +
                new string('c', 63) + "." +
                new string('d', domainLength - 192);

            return new string('a', 50) + "@" + domain;
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Create_EmptyValue_ReturnsEmptyFailure(string? rawValue)
        {
            var result = Email.Create(rawValue);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL.EMPTY_EMAIL",
                error.Code
            );

            Assert.Equal(
                "Email address is empty or missing. Owner: 'Email'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("example.com")]
        [InlineData("@example.com")]
        [InlineData("serhii@")]
        [InlineData("@")]
        [InlineData("serhii@@example.com")]
        [InlineData("serhii@example@com")]
        public void Create_InvalidAtSignStructure_ReturnsEmailFormatFailure(string value)
        {
            var result = Email.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL.INVALID_FORMAT_EMAIL",
                error.Code
            );

            Assert.Equal(
                "Email address has an invalid structure. It must contain exactly one '@' separator, " +
                "non-empty local and domain parts, and no whitespace. Owner: 'Email'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("ser hii@example.com")]
        [InlineData("serhii@exam ple.com")]
        [InlineData("serhii @example.com")]
        [InlineData("serhii@ example.com")]
        [InlineData("ser\thii@example.com")]
        [InlineData("serhii@example.\ncom")]
        [InlineData("serhii@\u00A0example.com")]
        public void Create_InternalWhitespace_ReturnsEmailFormatFailure(string value)
        {
            var result = Email.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL.INVALID_FORMAT_EMAIL",
                error.Code
            );
        }

        [Theory]
        [InlineData("serhii@example.com", "serhii", "example.com")]
        [InlineData("a.b@sub.example.com", "a.b", "sub.example.com")]
        [InlineData("my-user@my-site.com", "my-user", "my-site.com")]
        [InlineData("00@123.com", "00", "123.com")]
        [InlineData("ab@a.bc", "ab", "a.bc")]
        public void Create_ValidEmail_SetsParts(
            string value,
            string expectedLocal,
            string expectedDomain)
        {
            var result = Email.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                expectedLocal,
                result.Value.EmailLocalPart.Value
            );

            Assert.Equal(
                expectedDomain,
                result.Value.EmailDomainPart.Value
            );
        }

        [Theory]
        [InlineData(" serhii@example.com")]
        [InlineData("serhii@example.com ")]
        [InlineData("  serhii@example.com  ")]
        [InlineData("\tserhii@example.com\r\n")]
        public void Create_SurroundingWhitespace_TrimsEmail(string value)
        {
            var result = Email.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                "serhii",
                result.Value.EmailLocalPart.Value
            );

            Assert.Equal(
                "example.com",
                result.Value.EmailDomainPart.Value
            );
        }

        [Theory]
        [InlineData("SERHII@example.com")]
        [InlineData("serhii@EXAMPLE.COM")]
        [InlineData("SeRhIi@ExAmPlE.CoM")]
        public void Create_UppercaseParts_NormalizesBothParts(string value)
        {
            var result = Email.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                "serhii",
                result.Value.EmailLocalPart.Value
            );

            Assert.Equal(
                "example.com",
                result.Value.EmailDomainPart.Value
            );
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("uk-UA")]
        [InlineData("tr-TR")]
        public void Create_DifferentCultures_UsesInvariantLowercase(string cultureName)
        {
            var previousCulture = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(cultureName);

                var result = Email.Create("IL@IL.COM");

                Assert.True(result.IsSuccess);
                Assert.Empty(result.Errors);

                Assert.Equal(
                    "il",
                    result.Value.EmailLocalPart.Value
                );

                Assert.Equal(
                    "il.com",
                    result.Value.EmailDomainPart.Value
                );
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }

        [Theory]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(253)]
        [InlineData(254)]
        public void Create_ValidTotalLength_Succeeds(int length)
        {
            var value = length switch
            {
                7 => "ab@a.bc",
                8 => "abc@a.bc",
                _ => BuildEmailOfLength(length)
            };

            var result = Email.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                length,
                result.Value.GetFullEmail().Length
            );
        }

        [Theory]
        [InlineData(255)]
        [InlineData(256)]
        public void Create_TotalLengthExceedsMaximum_ReturnsEmailTooLongFailure(int length)
        {
            var value = BuildEmailOfLength(length);

            var result = Email.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL.TOO_LONG_EMAIL",
                error.Code
            );

            Assert.Equal(
                $"Email address length '{length}' exceeds the maximum allowed length of '254'. Owner: 'Email'.",
                error.Message
            );
        }

        [Fact]
        public void Create_PaddedMaximumLengthEmail_Succeeds()
        {
            var canonical = BuildEmailOfLength(254);
            var rawValue = " " + canonical + " ";

            var result = Email.Create(rawValue);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                canonical,
                result.Value.GetFullEmail()
            );

            Assert.Equal(
                254,
                result.Value.GetFullEmail().Length
            );
        }

        [Fact]
        public void Create_PaddedTooLongEmail_ReportsTrimmedLength()
        {
            var canonical = BuildEmailOfLength(255);
            var rawValue = " " + canonical + " ";

            var result = Email.Create(rawValue);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL.TOO_LONG_EMAIL",
                error.Code
            );

            Assert.Equal(
                "Email address length '255' exceeds the maximum allowed length of '254'. Owner: 'Email'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("a..b@example.com")]
        [InlineData(".ab@example.com")]
        public void Create_InvalidLocalFormat_PropagatesLocalFailure(string value)
        {
            var result = Email.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL_LOCAL.INVALID_FORMAT_EMAIL_LOCAL",
                error.Code
            );

            Assert.Equal(
                "Email local part has an invalid format. Owner: 'EmailLocalPart'.",
                error.Message
            );
        }

        [Fact]
        public void Create_TooShortLocalPart_PropagatesLocalFailure()
        {
            var result = Email.Create("a@example.com");

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL_LOCAL.TOO_SHORT_EMAIL_LOCAL",
                error.Code
            );

            Assert.Contains(
                "'1'",
                error.Message
            );

            Assert.Contains(
                "'2'",
                error.Message
            );
        }

        [Fact]
        public void Create_TooLongLocalPart_PropagatesLocalFailure()
        {
            var value = new string('a', 51) + "@example.com";

            var result = Email.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL_LOCAL.TOO_LONG_EMAIL_LOCAL",
                error.Code
            );

            Assert.Contains(
                "'51'",
                error.Message
            );

            Assert.Contains(
                "'50'",
                error.Message
            );
        }

        [Theory]
        [InlineData("serhii@example")]
        [InlineData("serhii@-example.com")]
        [InlineData("serhii@example.c")]
        public void Create_InvalidDomainFormat_PropagatesDomainFailure(string value)
        {
            var result = Email.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL_DOMAIN.INVALID_FORMAT_EMAIL_DOMAIN",
                error.Code
            );

            Assert.Equal(
                "Email domain has an invalid format. Owner: 'EmailDomainPart'!",
                error.Message
            );
        }

        [Theory]
        [InlineData("serhii@mail.ru")]
        [InlineData("serhii@yandex.ru")]
        public void Create_BlockedDomain_PropagatesBusinessRuleFailure(string value)
        {
            var result = Email.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.BusinessRule,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL_DOMAIN.NOT_ALLOWED_DOMAIN_PART",
                error.Code
            );

            Assert.Equal(
                "Email domain is not allowed. Owner: 'EmailDomainPart'!",
                error.Message
            );
        }

        [Fact]
        public void Create_PaddedMixedCaseBlockedEmail_PropagatesBusinessRuleFailure()
        {
            var result = Email.Create(" SeRhIi@MAIL.RU ");

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.BusinessRule,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL_DOMAIN.NOT_ALLOWED_DOMAIN_PART",
                error.Code
            );
        }

        [Theory]
        [InlineData("a..b@example", "EMAIL_LOCAL.INVALID_FORMAT_EMAIL_LOCAL")]
        [InlineData("a@mail.ru", "EMAIL_LOCAL.TOO_SHORT_EMAIL_LOCAL")]
        public void Create_InvalidLocalAndDomain_ReturnsLocalFailure(
            string value,
            string expectedCode)
        {
            var result = Email.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                expectedCode,
                error.Code
            );
        }

        [Fact]
        public void Create_InvalidStructureAndTooLong_ReturnsEmailFormatFailure()
        {
            var value = new string('a', 255);

            var result = Email.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL.INVALID_FORMAT_EMAIL",
                error.Code
            );
        }

        [Fact]
        public void Create_TooLongEmailAndInvalidLocal_ReturnsEmailTooLongFailure()
        {
            var value = new string('a', 255) + "@example.com";

            var result = Email.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL.TOO_LONG_EMAIL",
                error.Code
            );

            Assert.Equal(
                "Email address length '267' exceeds the maximum allowed length of '254'. Owner: 'Email'.",
                error.Message
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Restore_EmptyValue_ThrowsDomainDataInconsistencyException(string? storedValue)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore(storedValue!)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Field"]
            );
            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("example.com")]
        [InlineData("@example.com")]
        [InlineData("serhii@")]
        [InlineData("@")]
        [InlineData("serhii@@example.com")]
        [InlineData("serhii@example@com")]
        public void Restore_InvalidAtSignStructure_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "String",
                exception.Context["ValueType"]
            );
        }

        [Theory]
        [InlineData("ser hii@example.com")]
        [InlineData("serhii@exam ple.com")]
        [InlineData("serhii @example.com")]
        [InlineData("serhii@ example.com")]
        [InlineData("ser\thii@example.com")]
        [InlineData("serhii@example.\ncom")]
        [InlineData("serhii@\u00A0example.com")]
        [InlineData(" serhii@example.com")]
        [InlineData("serhii@example.com ")]
        [InlineData("  serhii@example.com  ")]
        [InlineData("\tserhii@example.com\r\n")]
        [InlineData("serhii@example.com\n")]
        public void Restore_Whitespace_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("serhii@example.com", "serhii", "example.com")]
        [InlineData("a.b@sub.example.com", "a.b", "sub.example.com")]
        [InlineData("my-user@my-site.com", "my-user", "my-site.com")]
        [InlineData("00@123.com", "00", "123.com")]
        [InlineData("ab@a.bc", "ab", "a.bc")]
        public void Restore_CanonicalEmail_SetsParts(
            string value,
            string expectedLocal,
            string expectedDomain)
        {
            var email = Email.Restore(value);

            Assert.Equal(
                expectedLocal,
                email.EmailLocalPart.Value
            );

            Assert.Equal(
                expectedDomain,
                email.EmailDomainPart.Value
            );
        }

        [Theory]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(253)]
        [InlineData(254)]
        public void Restore_CanonicalEmail_PreservesBoundaryLength(int length)
        {
            var value = length switch
            {
                7 => "ab@a.bc",
                8 => "abc@a.bc",
                _ => BuildEmailOfLength(length)
            };

            var email = Email.Restore(value);

            Assert.Equal(
                length,
                email.GetFullEmail().Length
            );
        }

        [Theory]
        [InlineData(255)]
        [InlineData(256)]
        public void Restore_TotalLengthExceedsMaximum_ThrowsDomainDataInconsistencyException(int length)
        {
            var value = BuildEmailOfLength(length);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                length,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                254,
                exception.Context["Custom_MaximumLength"]
            );
        }

        [Theory]
        [InlineData("a..b@example.com")]
        [InlineData(".ab@example.com")]
        public void Restore_InvalidLocalFormat_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "EmailLocalPart",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "String",
                exception.Context["ValueType"]
            );
        }

        [Fact]
        public void Restore_TooShortLocalPart_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore("a@example.com")
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "EmailLocalPart",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                1,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                2,
                exception.Context["Custom_MinimumLength"]
            );
        }

        [Fact]
        public void Restore_TooLongLocalPart_ThrowsDomainDataInconsistencyException()
        {
            var value = new string('a', 51) + "@example.com";

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "EmailLocalPart",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                51,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                50,
                exception.Context["Custom_MaximumLength"]
            );
        }

        [Theory]
        [InlineData("serhii@example")]
        [InlineData("serhii@-example.com")]
        [InlineData("serhii@example.c")]
        public void Restore_InvalidDomainFormat_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "EmailDomainPart",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "String",
                exception.Context["ValueType"]
            );
        }

        [Theory]
        [InlineData("serhii@mail.ru", "serhii", "mail.ru")]
        [InlineData("serhii@yandex.ru", "serhii", "yandex.ru")]
        public void Restore_BlockedDomain_PreservesEmail(
            string value,
            string expectedLocal,
            string expectedDomain)
        {
            var email = Email.Restore(value);

            Assert.Equal(
                expectedLocal,
                email.EmailLocalPart.Value
            );

            Assert.Equal(
                expectedDomain,
                email.EmailDomainPart.Value
            );
        }

        [Theory]
        [InlineData("SERHII@example.com", "EmailLocalPart")]
        [InlineData("serhii@EXAMPLE.COM", "EmailDomainPart")]
        [InlineData("serhii@MAIL.RU", "EmailDomainPart")]
        public void Restore_NonCanonicalPart_ThrowsDomainDataInconsistencyException(
            string value,
            string expectedType)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                expectedType,
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Restore_InvalidLocalAndDomain_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore("a..b@example")
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                "EmailLocalPart",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );
        }

        [Fact]
        public void Restore_InvalidStructureAndTooLong_ThrowsDomainDataInconsistencyException()
        {
            var value = new string('a', 255);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Field"]
            );
        }

        [Fact]
        public void Restore_TooLongEmailAndInvalidLocal_ThrowsDomainDataInconsistencyException()
        {
            var value = new string('a', 255) + "@example.com";

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Email.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Field"]
            );

            Assert.Equal(
                267,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                254,
                exception.Context["Custom_MaximumLength"]
            );
        }

        [Theory]
        [InlineData("serhii@example.com")]
        [InlineData("a.b@sub.example.com")]
        [InlineData("ab@a.bc")]
        public void GetFullEmail_ValidEmail_ReturnsJoinedParts(string value)
        {
            var email = Email.Restore(value);

            Assert.Equal(
                value,
                email.GetFullEmail()
            );
        }

        [Fact]
        public void GetEqualityComponents_ValidEmail_ReturnsLocalThenDomain()
        {
            var email = Email.Restore("serhii@example.com");

            var components = email.GetEqualityComponents().ToArray();

            Assert.Equal(
                2,
                components.Length
            );

            Assert.Same(
                email.EmailLocalPart,
                components[0]
            );

            Assert.Same(
                email.EmailDomainPart,
                components[1]
            );
        }

        [Theory]
        [InlineData("serhii@example.com", "petrenko@example.com")]
        [InlineData("serhii@example.com", "serhii@example.org")]
        public void Equals_OnePartDiffers_ReturnsFalse(
            string firstValue,
            string secondValue)
        {
            var first = Email.Restore(firstValue);
            var second = Email.Restore(secondValue);

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }

        [Fact]
        public void CreateThenRestore_NormalizedEmail_PreservesValuesAndEquality()
        {
            var createResult = Email.Create(" SeRhIi@ExAmPlE.CoM ");

            Assert.True(createResult.IsSuccess);
            Assert.Empty(createResult.Errors);

            var created = createResult.Value;
            var restored = Email.Restore(created.GetFullEmail());

            Assert.Equal(
                "serhii",
                created.EmailLocalPart.Value
            );

            Assert.Equal(
                "example.com",
                created.EmailDomainPart.Value
            );

            Assert.Equal(
                "serhii",
                restored.EmailLocalPart.Value
            );

            Assert.Equal(
                "example.com",
                restored.EmailDomainPart.Value
            );

            Assert.Equal(
                "serhii@example.com",
                created.GetFullEmail()
            );

            Assert.Equal("serhii@example.com", restored.GetFullEmail());
            Assert.NotSame(
                created,
                restored
            );

            Assert.True(created.Equals((object)restored));
            Assert.True(restored.Equals((object)created));

            Assert.Equal(
                created.GetHashCode(),
                restored.GetHashCode()
            );
        }
    }
}
