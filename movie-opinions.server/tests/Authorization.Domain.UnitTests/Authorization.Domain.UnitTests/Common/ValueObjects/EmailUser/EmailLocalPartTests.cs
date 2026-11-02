using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using System.Globalization;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.EmailUser
{
    public class EmailLocalPartTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Create_EmptyValue_ReturnsEmptyFailure(string? rawValue)
        {
            var result = EmailLocalPart.Create(rawValue!);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "EMAIL_LOCAL.EMPTY_EMAIL_LOCAL",
                error.Code
            );

            Assert.Equal(
                "Email local part is empty or missing. Owner: 'EmailLocalPart'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("00")]
        [InlineData("a0")]
        [InlineData("serhii")]
        [InlineData("serhii01")]
        [InlineData("serhii.petrenko")]
        [InlineData("serhii_petrenko")]
        [InlineData("serhii-petrenko")]
        [InlineData("a.b_c-d0")]
        public void Create_ValidFormat_PreservesValue(string value)
        {
            var result = EmailLocalPart.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(49)]
        [InlineData(50)]
        public void Create_ValidLength_Succeeds(int length)
        {
            var value = new string('a', length);

            var result = EmailLocalPart.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("a")]
        [InlineData("1")]
        public void Create_TooShortValue_ReturnsTooShortFailure(string value)
        {
            var result = EmailLocalPart.Create(value);

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

            Assert.Equal(
                "Email local part length '1' is shorter than the minimum allowed length of '2'. Owner: 'EmailLocalPart'.",
                error.Message
            );
        }

        [Theory]
        [InlineData(51)]
        [InlineData(100)]
        public void Create_TooLongValue_ReturnsTooLongFailure(int length)
        {
            var value = new string('a', length);

            var result = EmailLocalPart.Create(value);

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

            Assert.Equal(
                $"Email local part length '{length}' exceeds the maximum allowed length of '50'. Owner: 'EmailLocalPart'.",
                error.Message
            );
        }

        [Theory]
        [InlineData(".ab")]
        [InlineData("_ab")]
        [InlineData("-ab")]
        [InlineData("ab.")]
        [InlineData("ab_")]
        [InlineData("ab-")]
        [InlineData("a..b")]
        [InlineData("a._b")]
        [InlineData("a.-b")]
        [InlineData("a_.b")]
        [InlineData("a__b")]
        [InlineData("a_-b")]
        [InlineData("a-.b")]
        [InlineData("a-_b")]
        [InlineData("a--b")]
        public void Create_InvalidSeparatorPlacement_ReturnsInvalidFormatFailure(string value)
        {
            var result = EmailLocalPart.Create(value);

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

        [Theory]
        [InlineData("ab+cd")]
        [InlineData("ab@cd")]
        [InlineData("ab/cd")]
        [InlineData("ab\\cd")]
        [InlineData("ab!cd")]
        [InlineData("ab:cd")]
        [InlineData("ab'cd")]
        [InlineData("сергій")]
        [InlineData("café")]
        [InlineData("ab🙂cd")]
        public void Create_ForbiddenCharacters_ReturnsInvalidFormatFailure(string value)
        {
            var result = EmailLocalPart.Create(value);

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
        }

        [Theory]
        [InlineData("ab cd")]
        [InlineData("ab\tcd")]
        [InlineData("ab\ncd")]
        [InlineData("ab\rcd")]
        public void Create_InternalWhitespace_ReturnsInvalidFormatFailure(string value)
        {
            var result = EmailLocalPart.Create(value);

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
        }

        [Theory]
        [InlineData(" serhii", "serhii")]
        [InlineData("serhii ", "serhii")]
        [InlineData(" serhii ", "serhii")]
        [InlineData("\tserhii\r\n", "serhii")]
        public void Create_SurroundingWhitespace_TrimsValue(
            string rawValue,
            string expected)
        {
            var result = EmailLocalPart.Create(rawValue);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                expected,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("SERHII", "serhii")]
        [InlineData("SeRhIi", "serhii")]
        [InlineData("SERHII.PETRENKO", "serhii.petrenko")]
        public void Create_UppercaseLetters_ConvertsToLowercase(
            string rawValue,
            string expected)
        {
            var result = EmailLocalPart.Create(rawValue);

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

                var result = EmailLocalPart.Create("IL");

                Assert.True(result.IsSuccess);
                Assert.Empty(result.Errors);

                Assert.Equal(
                    "il",
                    result.Value.Value
                );
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }

        [Theory]
        [InlineData(2)]
        [InlineData(50)]
        public void Create_PaddedBoundaryValue_ValidatesNormalizedLength(int length)
        {
            var rawValue = " " + new string('A', length) + " ";
            var expected = new string('a', length);

            var result = EmailLocalPart.Create(rawValue);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                expected,
                result.Value.Value
            );
        }

        [Fact]
        public void Create_PaddedTooShortValue_ReportsNormalizedLength()
        {
            var result = EmailLocalPart.Create(" a ");

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

            Assert.Equal(
                "Email local part length '1' is shorter than the minimum allowed length of '2'. Owner: 'EmailLocalPart'.",
                error.Message
            );
        }

        [Fact]
        public void Create_PaddedTooLongValue_ReportsNormalizedLength()
        {
            var rawValue = " " + new string('a', 51) + " ";

            var result = EmailLocalPart.Create(rawValue);

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

            Assert.Equal(
                "Email local part length '51' exceeds the maximum allowed length of '50'. Owner: 'EmailLocalPart'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("#")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa@")]
        public void Create_InvalidFormatAndInvalidLength_ReturnsFormatFailure(string value)
        {
            var result = EmailLocalPart.Create(value);

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
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Restore_EmptyValue_ThrowsDomainDataInconsistencyException(string? storedValue)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLocalPart.Restore(storedValue!)
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
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("00")]
        [InlineData("a0")]
        [InlineData("serhii")]
        [InlineData("serhii01")]
        [InlineData("serhii.petrenko")]
        [InlineData("serhii_petrenko")]
        [InlineData("serhii-petrenko")]
        [InlineData("a.b_c-d0")]
        public void Restore_CanonicalValue_PreservesValue(string value)
        {
            var localPart = EmailLocalPart.Restore(value);

            Assert.Equal(
                value,
                localPart.Value
            );
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(49)]
        [InlineData(50)]
        public void Restore_CanonicalValue_PreservesBoundaryLength(int length)
        {
            var value = new string('a', length);

            var localPart = EmailLocalPart.Restore(value);

            Assert.Equal(value, localPart.Value);
        }

        [Theory]
        [InlineData(".ab")]
        [InlineData("_ab")]
        [InlineData("-ab")]
        [InlineData("ab.")]
        [InlineData("ab_")]
        [InlineData("ab-")]
        [InlineData("a..b")]
        [InlineData("a._b")]
        [InlineData("a.-b")]
        [InlineData("a_.b")]
        [InlineData("a__b")]
        [InlineData("a_-b")]
        [InlineData("a-.b")]
        [InlineData("a-_b")]
        [InlineData("a--b")]
        [InlineData("ab+cd")]
        [InlineData("ab@cd")]
        [InlineData("ab/cd")]
        [InlineData("ab\\cd")]
        [InlineData("ab!cd")]
        [InlineData("ab:cd")]
        [InlineData("ab'cd")]
        [InlineData("сергій")]
        [InlineData("café")]
        [InlineData("ab🙂cd")]
        [InlineData("ab cd")]
        [InlineData("ab\tcd")]
        [InlineData("ab\ncd")]
        [InlineData("ab\rcd")]
        public void Restore_InvalidFormat_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLocalPart.Restore(value)
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

        [Theory]
        [InlineData("a")]
        [InlineData("1")]
        public void Restore_TooShortValue_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLocalPart.Restore(value)
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

        [Theory]
        [InlineData(51)]
        [InlineData(100)]
        public void Restore_TooLongValue_ThrowsDomainDataInconsistencyException(int length)
        {
            var value = new string('a', length);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLocalPart.Restore(value)
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
                length,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                50,
                exception.Context["Custom_MaximumLength"]
            );
        }

        [Theory]
        [InlineData("SERHII")]
        [InlineData("SeRhIi")]
        [InlineData("SERHII.PETRENKO")]
        public void Restore_UppercaseLetters_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLocalPart.Restore(value)
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
        }

        [Theory]
        [InlineData(" serhii")]
        [InlineData("serhii ")]
        [InlineData(" serhii ")]
        [InlineData("\tserhii\r\n")]
        [InlineData("serhii\n")]
        public void Restore_SurroundingWhitespace_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLocalPart.Restore(value)
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
        }

        [Theory]
        [InlineData("#")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa@")]
        public void Restore_InvalidFormatAndInvalidLength_ThrowsInvalidFormatException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLocalPart.Restore(value)
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

        [Fact]
        public void GetEqualityComponents_ValidValue_ReturnsSingleStringComponent()
        {
            var localPart = EmailLocalPart.Restore("serhii");

            var component = Assert.Single(localPart.GetEqualityComponents());

            Assert.Equal(
                "serhii",
                component
            );

            Assert.IsType<string>(component);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var first = EmailLocalPart.Restore("serhii");
            var second = EmailLocalPart.Restore("petrenko");

            Assert.False(first.Equals((object)second));

            Assert.False(second.Equals((object)first));
        }

        [Fact]
        public void CreateThenRestore_NormalizedValue_PreservesEqualityAndHashCode()
        {
            var createResult = EmailLocalPart.Create(" SeRhIi ");

            Assert.True(createResult.IsSuccess);

            var created = createResult.Value;
            var restored = EmailLocalPart.Restore(created.Value);

            Assert.Equal(
                "serhii",
                created.Value
            );

            Assert.Equal(
                "serhii",
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
    }
}
