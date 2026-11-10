using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using System.Globalization;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.EmailUser
{
    public class EmailDomainPartTests
    {
        #region Helpers
        // Перші мітки мають допустиму довжину, щоб перевіряти загальну межу домену.
        // The first labels have valid lengths to isolate the total domain length boundary.
        private static string BuildDomainOfLength(int length)
        {
            return
                new string('a', 63) + "." +
                new string('b', 63) + "." +
                new string('c', 63) + "." +
                new string('d', length - 192);
        }
        #endregion

        #region Create: format and length
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Create_EmptyValue_ReturnsEmptyFailure(string? rawValue)
        {
            var result = EmailDomainPart.Create(rawValue!);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL_DOMAIN.EMPTY_DOMAIN_PART",
                error.Code
            );

            Assert.Equal(
                "Email domain is empty or missing. Owner: 'EmailDomainPart'!",
                error.Message
            );
        }

        [Theory]
        [InlineData("example.com")]
        [InlineData("sub.example.com")]
        [InlineData("example.co.uk")]
        [InlineData("my-site.com")]
        [InlineData("a--b.com")]
        [InlineData("123.com")]
        [InlineData("1.co")]
        [InlineData("xn--e1afmkfd.com")]
        public void Create_ValidFormat_PreservesValue(string value)
        {
            var result = EmailDomainPart.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(62)]
        [InlineData(63)]
        public void Create_ValidLabelLength_Succeeds(int length)
        {
            var value = new string('a', length) + ".com";

            var result = EmailDomainPart.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Fact]
        public void Create_LabelLongerThan63Characters_ReturnsInvalidFormatFailure()
        {
            var value = new string('a', 64) + ".com";

            var result = EmailDomainPart.Create(value);

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
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(62)]
        [InlineData(63)]
        public void Create_ValidTopLevelDomainLength_Succeeds(int length)
        {
            var value = "a." + new string('b', length);

            var result = EmailDomainPart.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(64)]
        public void Create_InvalidTopLevelDomainLength_ReturnsInvalidFormatFailure(int length)
        {
            var value = "a." + new string('b', length);

            var result = EmailDomainPart.Create(value);

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
        }

        [Theory]
        [InlineData("example")]
        [InlineData(".com")]
        [InlineData("example.")]
        [InlineData(".example.com")]
        [InlineData("example..com")]
        [InlineData("sub..example.com")]
        [InlineData("-example.com")]
        [InlineData("example-.com")]
        [InlineData("sub.-example.com")]
        [InlineData("sub.example-.com")]
        [InlineData("example.123")]
        [InlineData("example.c0m")]
        [InlineData("example.co-m")]
        [InlineData("example.co_m")]
        [InlineData("ex_ample.com")]
        [InlineData("ex+ample.com")]
        [InlineData("ex@ample.com")]
        [InlineData("https://example.com")]
        [InlineData("example.com/path")]
        [InlineData("example.com:443")]
        [InlineData("ex ample.com")]
        [InlineData("example. com")]
        [InlineData("example .com")]
        [InlineData("домен.com")]
        [InlineData("example.укр")]
        [InlineData("café.com")]
        [InlineData("ex🙂ample.com")]
        [InlineData("exam\tple.com")]
        [InlineData("exam\nple.com")]
        public void Create_InvalidFormat_ReturnsInvalidFormatFailure(string value)
        {
            var result = EmailDomainPart.Create(value);

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
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(252)]
        [InlineData(253)]
        public void Create_ValidTotalLength_Succeeds(int length)
        {
            var value = length switch
            {
                4 => "a.bc",
                5 => "ab.cd",
                _ => BuildDomainOfLength(length)
            };

            var result = EmailDomainPart.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );

            Assert.Equal(
                length,
                result.Value.Value.Length
            );
        }

        [Theory]
        [InlineData("a")]
        [InlineData("ab")]
        [InlineData("abc")]
        [InlineData("a.b")]
        public void Create_NonEmptyValueShorterThanMinimum_ReturnsInvalidFormatFailure(string value)
        {
            var result = EmailDomainPart.Create(value);

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
        }

        [Theory]
        [InlineData(254)]
        [InlineData(255)]
        public void Create_TotalLengthExceedsMaximum_ReturnsTooLongFailure(int length)
        {
            var value = BuildDomainOfLength(length);

            var result = EmailDomainPart.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL_DOMAIN.TOO_LONG_EMAIL_DOMAIN",
                error.Code
            );

            Assert.Equal(
                $"Email domain length '{length}' exceeds the maximum allowed length of '253' characters. Owner: 'EmailDomainPart'!",
                error.Message
            );
        }
        #endregion

        #region Create: blocked domains
        [Theory]
        [InlineData("mail.ru")]
        [InlineData("yandex.ru")]
        [InlineData("bk.ru")]
        [InlineData("inbox.ru")]
        [InlineData("list.ru")]
        [InlineData("rambler.ru")]
        [InlineData("internet.ru")]
        [InlineData("xmail.ru")]
        public void Create_BlockedDomain_ReturnsNotAllowedFailure(string value)
        {
            var result = EmailDomainPart.Create(value);

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

        [Theory]
        [InlineData("MAIL.RU")]
        [InlineData("YaNdEx.Ru")]
        [InlineData(" MAIL.RU ")]
        [InlineData("\tYaNdEx.Ru\r\n")]
        public void Create_BlockedDomainWithDifferentCasingOrWhitespace_ReturnsNotAllowedFailure(string value)
        {
            var result = EmailDomainPart.Create(value);

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
        [InlineData("example.ru")]
        [InlineData("sub.mail.ru")]
        [InlineData("mail.ru.example.com")]
        [InlineData("notmail.ru")]
        [InlineData("mail-ru.com")]
        public void Create_DomainOutsideBlockedList_Succeeds(string value)
        {
            var result = EmailDomainPart.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }
        #endregion

        #region Create: normalization and validation priority
        [Theory]
        [InlineData(" example.com", "example.com")]
        [InlineData("example.com ", "example.com")]
        [InlineData(" example.com ", "example.com")]
        [InlineData("\texample.com\r\n", "example.com")]
        public void Create_SurroundingWhitespace_TrimsValue(
            string rawValue,
            string expected)
        {
            var result = EmailDomainPart.Create(rawValue);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                expected,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("EXAMPLE.COM", "example.com")]
        [InlineData("ExAmPlE.CoM", "example.com")]
        [InlineData("SUB.EXAMPLE.COM", "sub.example.com")]
        public void Create_UppercaseLetters_ConvertsToLowercase(
            string rawValue,
            string expected)
        {
            var result = EmailDomainPart.Create(rawValue);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                expected,
                result.Value.Value
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

                var result = EmailDomainPart.Create("IL.COM");

                Assert.True(result.IsSuccess);
                Assert.Empty(result.Errors);

                Assert.Equal(
                    "il.com",
                    result.Value.Value
                );
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }

        [Theory]
        [InlineData(4)]
        [InlineData(253)]
        public void Create_PaddedBoundaryValue_ValidatesNormalizedLength(int length)
        {
            var canonical = length == 4
                ? "a.bc"
                : BuildDomainOfLength(253);

            var rawValue = " " + canonical.ToUpperInvariant() + " ";

            var result = EmailDomainPart.Create(rawValue);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                canonical,
                result.Value.Value
            );
        }

        [Fact]
        public void Create_PaddedTooLongValue_ReportsNormalizedLength()
        {
            var canonical = BuildDomainOfLength(254);
            var rawValue = " " + canonical + " ";

            var result = EmailDomainPart.Create(rawValue);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL_DOMAIN.TOO_LONG_EMAIL_DOMAIN",
                error.Code
            );

            Assert.Equal(
                "Email domain length '254' exceeds the maximum allowed length of '253' characters. Owner: 'EmailDomainPart'!",
                error.Message
            );
        }

        [Theory]
        [InlineData("#")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.com")]
        public void Create_InvalidFormatAndInvalidLength_ReturnsFormatFailure(string value)
        {
            var result = EmailDomainPart.Create(value);

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
        }
        #endregion

        #region Restore: format and length
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Restore_EmptyValue_ThrowsDomainDataInconsistencyException(string? storedValue)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailDomainPart.Restore(storedValue!)
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
        }

        [Theory]
        [InlineData("example.com")]
        [InlineData("sub.example.com")]
        [InlineData("example.co.uk")]
        [InlineData("my-site.com")]
        [InlineData("a--b.com")]
        [InlineData("123.com")]
        [InlineData("1.co")]
        [InlineData("xn--e1afmkfd.com")]
        [InlineData("a.bc")]
        [InlineData("ab.cd")]
        public void Restore_CanonicalValue_PreservesValue(string value)
        {
            var domainPart = EmailDomainPart.Restore(value);

            Assert.Equal(
                value,
                domainPart.Value
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(62)]
        [InlineData(63)]
        public void Restore_CanonicalValue_PreservesLabelLength(int length)
        {
            var value = new string('a', length) + ".com";

            var domainPart = EmailDomainPart.Restore(value);

            Assert.Equal(value, domainPart.Value);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(62)]
        [InlineData(63)]
        public void Restore_CanonicalValue_PreservesTopLevelDomainLength(int length)
        {
            var value = "a." + new string('b', length);

            var domainPart = EmailDomainPart.Restore(value);

            Assert.Equal(value, domainPart.Value);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(252)]
        [InlineData(253)]
        public void Restore_CanonicalValue_PreservesTotalLength(int length)
        {
            var value = length switch
            {
                4 => "a.bc",
                5 => "ab.cd",
                _ => BuildDomainOfLength(length)
            };

            var domainPart = EmailDomainPart.Restore(value);

            Assert.Equal(value, domainPart.Value);
        }

        [Theory]
        [InlineData("a.b")]
        [InlineData("example")]
        [InlineData(".com")]
        [InlineData("example.")]
        [InlineData(".example.com")]
        [InlineData("example..com")]
        [InlineData("sub..example.com")]
        [InlineData("-example.com")]
        [InlineData("example-.com")]
        [InlineData("sub.-example.com")]
        [InlineData("sub.example-.com")]
        [InlineData("example.123")]
        [InlineData("example.c0m")]
        [InlineData("example.co-m")]
        [InlineData("example.co_m")]
        [InlineData("ex_ample.com")]
        [InlineData("ex+ample.com")]
        [InlineData("ex@ample.com")]
        [InlineData("https://example.com")]
        [InlineData("example.com/path")]
        [InlineData("example.com:443")]
        [InlineData("ex ample.com")]
        [InlineData("example. com")]
        [InlineData("example .com")]
        [InlineData("домен.com")]
        [InlineData("example.укр")]
        [InlineData("café.com")]
        [InlineData("ex🙂ample.com")]
        [InlineData("exam\tple.com")]
        [InlineData("exam\nple.com")]
        [InlineData("a")]
        [InlineData("ab")]
        [InlineData("abc")]
        public void Restore_InvalidFormat_ThrowsDomainDataInconsistencyException(string value)
        {
            AssertInvalidFormatOnRestore(value);
        }

        [Fact]
        public void Restore_LabelLongerThan63_ThrowsInvalidFormatException()
        {
            AssertInvalidFormatOnRestore(new string('a', 64) + ".com");
        }

        [Fact]
        public void Restore_TopLevelDomainLongerThan63_ThrowsInvalidFormatException()
        {
            AssertInvalidFormatOnRestore("a." + new string('b', 64));
        }

        #region Helpers: Restore diagnostics
        // Спільна перевірка діагностики для різних порушень формату домену.
        // Shared diagnostic assertions for different domain format violations.
        private static void AssertInvalidFormatOnRestore(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailDomainPart.Restore(value)
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
        #endregion

        [Theory]
        [InlineData(254)]
        [InlineData(255)]
        public void Restore_TotalLengthExceedsMaximum_ThrowsDomainDataInconsistencyException(int length)
        {
            var value = BuildDomainOfLength(length);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailDomainPart.Restore(value)
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
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                length,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                253,
                exception.Context["Custom_MaximumLength"]
            );
        }
        #endregion

        #region Restore: stored business state and canonical form
        // Restore зберігає історичний домен попри заборону для нових значень.
        // Restore preserves a historical domain despite the restriction on new values.
        [Theory]
        [InlineData("mail.ru")]
        [InlineData("yandex.ru")]
        [InlineData("bk.ru")]
        [InlineData("inbox.ru")]
        [InlineData("list.ru")]
        [InlineData("rambler.ru")]
        [InlineData("internet.ru")]
        [InlineData("xmail.ru")]
        public void Restore_BlockedDomain_PreservesValue(string value)
        {
            var domainPart = EmailDomainPart.Restore(value);

            Assert.Equal(
                value,
                domainPart.Value
            );
        }

        [Theory]
        [InlineData("EXAMPLE.COM")]
        [InlineData("ExAmPlE.CoM")]
        [InlineData("SUB.EXAMPLE.COM")]
        [InlineData("MAIL.RU")]
        public void Restore_UppercaseLetters_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailDomainPart.Restore(value)
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
        }

        [Theory]
        [InlineData(" example.com")]
        [InlineData("example.com ")]
        [InlineData(" example.com ")]
        [InlineData("\texample.com\r\n")]
        [InlineData("example.com\n")]
        public void Restore_SurroundingWhitespace_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailDomainPart.Restore(value)
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
        }

        [Theory]
        [InlineData("#")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" +
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaa.com")]
        public void Restore_InvalidFormatAndInvalidLength_ThrowsInvalidFormatException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailDomainPart.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );
        }
        #endregion

        #region Equality and hash code
        [Fact]
        public void GetEqualityComponents_ValidValue_ReturnsSingleStringComponent()
        {
            var domainPart = EmailDomainPart.Restore("example.com");

            var component = Assert.Single(domainPart.GetEqualityComponents());

            Assert.Equal(
                "example.com",
                component
            );

            Assert.IsType<string>(component);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var first = EmailDomainPart.Restore("example.com");
            var second = EmailDomainPart.Restore("example.org");

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion

        #region Create and Restore
        [Fact]
        public void CreateThenRestore_NormalizedValue_PreservesEqualityAndHashCode()
        {
            var createResult = EmailDomainPart.Create(" ExAmPlE.CoM ");

            Assert.True(createResult.IsSuccess);
            Assert.Empty(createResult.Errors);

            var created = createResult.Value;
            var restored = EmailDomainPart.Restore(created.Value);

            Assert.Equal(
                "example.com",
                created.Value
            );

            Assert.Equal(
                "example.com",
                restored.Value
            );

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
        #endregion
    }
}
