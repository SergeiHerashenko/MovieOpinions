using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.PhoneUser;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.PhoneUser
{
    public class PhoneCountryCodeTests
    {
        #region Create
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Create_EmptyValue_ReturnsEmptyFailure(string? rawValue)
        {
            var result = PhoneCountryCode.Create(rawValue!);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "COUNTRY_CODE.EMPTY_COUNTRY_CODE",
                error.Code
            );

            Assert.Equal(
                "Phone country calling code is empty or missing. Owner: 'PhoneCountryCode'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("+1")]
        [InlineData("+9")]
        [InlineData("+7")]
        [InlineData("+12")]
        [InlineData("+44")]
        [InlineData("+380")]
        [InlineData("+999")]
        [InlineData("+10")]
        [InlineData("+100")]
        [InlineData("+101")]
        public void Create_ValidCode_PreservesValue(string value)
        {
            var result = PhoneCountryCode.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("1")]
        [InlineData("380")]
        [InlineData("00380")]
        [InlineData("-380")]
        public void Create_MissingPlus_ReturnsInvalidFormatFailure(string value)
        {
            var result = PhoneCountryCode.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "COUNTRY_CODE.INVALID_FORMAT_COUNTRY_CODE",
                error.Code
            );
        }

        [Theory]
        [InlineData("++1")]
        [InlineData("+1+")]
        [InlineData("1+")]
        [InlineData("1+2")]
        [InlineData("380+")]
        [InlineData("＋380")]
        public void Create_InvalidPlusPlacement_ReturnsInvalidFormatFailure(string value)
        {
            var result = PhoneCountryCode.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "COUNTRY_CODE.INVALID_FORMAT_COUNTRY_CODE",
                error.Code
            );
        }

        [Theory]
        [InlineData("+0")]
        [InlineData("+01")]
        [InlineData("+038")]
        [InlineData("+000")]
        public void Create_LeadingZero_ReturnsInvalidFormatFailure(string value)
        {
            var result = PhoneCountryCode.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "COUNTRY_CODE.INVALID_FORMAT_COUNTRY_CODE",
                error.Code
            );
        }

        [Theory]
        [InlineData("+a")]
        [InlineData("+3a")]
        [InlineData("+38O")]
        [InlineData("+-1")]
        [InlineData("+1.2")]
        [InlineData("+1-2")]
        [InlineData("+1(2)")]
        [InlineData("+1/2")]
        [InlineData("+1_2")]
        [InlineData("+1🙂")]
        public void Create_ForbiddenCharacters_ReturnsInvalidFormatFailure(string value)
        {
            var result = PhoneCountryCode.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "COUNTRY_CODE.INVALID_FORMAT_COUNTRY_CODE",
                error.Code
            );

            Assert.Equal(
                "Phone country calling code has an invalid format. It must begin with '+'" +
                " and contain only ASCII digits, with the first digit from '1' to '9'. Owner: 'PhoneCountryCode'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("+٣٨٠")]
        [InlineData("+۳۸۰")]
        [InlineData("+３８０")]
        [InlineData("+1٢")]
        public void Create_NonAsciiDigits_ReturnsInvalidFormatFailure(string value)
        {
            var result = PhoneCountryCode.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "COUNTRY_CODE.INVALID_FORMAT_COUNTRY_CODE",
                error.Code
            );
        }

        [Theory]
        [InlineData("+ 1")]
        [InlineData("+1 2")]
        [InlineData("+1\t2")]
        [InlineData("+1\n2")]
        [InlineData("+3\r80")]
        [InlineData("+\u00A01")]
        public void Create_InternalWhitespace_ReturnsInvalidFormatFailure(string value)
        {
            var result = PhoneCountryCode.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "COUNTRY_CODE.INVALID_FORMAT_COUNTRY_CODE",
                error.Code
            );
        }

        [Fact]
        public void Create_PlusOnly_ReturnsTooShortFailure()
        {
            var result = PhoneCountryCode.Create("+");

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "COUNTRY_CODE.TOO_SHORT_COUNTRY_CODE",
                error.Code
            );

            Assert.Equal(
                "Phone country calling code length '1' is shorter than the minimum " +
                "allowed length of '2'. Owner: 'PhoneCountryCode'.",
                error.Message
            );
        }

        [Theory]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(20)]
        public void Create_TooLongCode_ReturnsTooLongFailure(int length)
        {
            var value = "+" + new string('1', length - 1);

            var result = PhoneCountryCode.Create(value);

            Assert.True(result.IsFailure);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ErrorType.Validation, error.ErrorType);
            Assert.Equal("COUNTRY_CODE.TOO_LONG_COUNTRY_CODE", error.Code);
            Assert.Equal(
                $"Phone country calling code length '{length}' exceeds the maximum allowed length of '4'. Owner: 'PhoneCountryCode'.",
                error.Message
            );
        }

        [Theory]
        [InlineData(" +380", "+380")]
        [InlineData("+380 ", "+380")]
        [InlineData(" +380 ", "+380")]
        [InlineData("\t+380\r\n", "+380")]
        [InlineData("\u00A0+380\u00A0", "+380")]
        public void Create_SurroundingWhitespace_TrimsValue(
            string rawValue,
            string expected)
        {
            var result = PhoneCountryCode.Create(rawValue);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                expected,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(" +1 ", "+1")]
        [InlineData(" +123 ", "+123")]
        public void Create_PaddedBoundaryValue_ValidatesTrimmedLength(
            string rawValue,
            string expected)
        {
            var result = PhoneCountryCode.Create(rawValue);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                expected,
                result.Value.Value
            );
        }

        [Fact]
        public void Create_PaddedPlusOnly_ReportsTrimmedLength()
        {
            var result = PhoneCountryCode.Create(" + ");

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "COUNTRY_CODE.TOO_SHORT_COUNTRY_CODE",
                error.Code
            );

            Assert.Equal(
                "Phone country calling code length '1' is shorter than the minimum allowed length of '2'. Owner: 'PhoneCountryCode'.",
                error.Message
            );
        }

        [Fact]
        public void Create_PaddedTooLongCode_ReportsTrimmedLength()
        {
            var result = PhoneCountryCode.Create(" +1234 ");

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "COUNTRY_CODE.TOO_LONG_COUNTRY_CODE",
                error.Code
            );

            Assert.Equal(
                "Phone country calling code length '5' exceeds the maximum allowed length of '4'. Owner: 'PhoneCountryCode'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("＋")]
        [InlineData("12345")]
        [InlineData("+0123")]
        [InlineData("+123a")]
        public void Create_InvalidFormatAndInvalidLength_ReturnsFormatFailure(string value)
        {
            var result = PhoneCountryCode.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "COUNTRY_CODE.INVALID_FORMAT_COUNTRY_CODE",
                error.Code
            );
        }
        #endregion

        #region Restore
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Restore_EmptyValue_ThrowsDomainDataInconsistencyException(string? storedValue)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneCountryCode.Restore(storedValue!)
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
                "PhoneCountryCode",
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
        [InlineData("+1")]
        [InlineData("+9")]
        [InlineData("+7")]
        [InlineData("+12")]
        [InlineData("+44")]
        [InlineData("+380")]
        [InlineData("+999")]
        [InlineData("+10")]
        [InlineData("+100")]
        [InlineData("+101")]
        public void Restore_ValidCode_PreservesValue(string value)
        {
            var countryCode = PhoneCountryCode.Restore(value);

            Assert.Equal(
                value,
                countryCode.Value
            );
        }

        [Theory]
        [InlineData("1")]
        [InlineData("380")]
        [InlineData("00380")]
        [InlineData("-380")]
        [InlineData("++1")]
        [InlineData("+1+")]
        [InlineData("1+")]
        [InlineData("1+2")]
        [InlineData("380+")]
        [InlineData("＋380")]
        [InlineData("+0")]
        [InlineData("+01")]
        [InlineData("+038")]
        [InlineData("+000")]
        [InlineData("+a")]
        [InlineData("+3a")]
        [InlineData("+38O")]
        [InlineData("+-1")]
        [InlineData("+1.2")]
        [InlineData("+1-2")]
        [InlineData("+1(2)")]
        [InlineData("+1/2")]
        [InlineData("+1_2")]
        [InlineData("+1🙂")]
        [InlineData("+٣٨٠")]
        [InlineData("+۳۸۰")]
        [InlineData("+３８０")]
        [InlineData("+1٢")]
        [InlineData("+ 1")]
        [InlineData("+1 2")]
        [InlineData("+1\t2")]
        [InlineData("+1\n2")]
        [InlineData("+3\r80")]
        [InlineData("+\u00A01")]
        public void Restore_InvalidFormat_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneCountryCode.Restore(value)
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
                "PhoneCountryCode",
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

        // Restore вимагає канонічний код і не виконує нормалізацію Create.
        // Restore requires a canonical code and does not apply Create normalization.
        [Theory]
        [InlineData(" +380")]
        [InlineData("+380 ")]
        [InlineData(" +380 ")]
        [InlineData("\t+380\r\n")]
        [InlineData("\u00A0+380\u00A0")]
        public void Restore_SurroundingWhitespace_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneCountryCode.Restore(value)
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
                "PhoneCountryCode",
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
        public void Restore_PlusOnly_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneCountryCode.Restore("+")
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
                "PhoneCountryCode",
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
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(20)]
        public void Restore_TooLongCode_ThrowsDomainDataInconsistencyException(int length)
        {
            var value = "+" + new string('1', length - 1);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneCountryCode.Restore(value)
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
                "PhoneCountryCode",
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
                4,
                exception.Context["Custom_MaximumLength"]
            );
        }

        [Theory]
        [InlineData("＋")]
        [InlineData("12345")]
        [InlineData("+0123")]
        [InlineData("+123a")]
        public void Restore_InvalidFormatAndInvalidLength_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneCountryCode.Restore(value)
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
        public void GetEqualityComponents_ValidCode_ReturnsSingleStringComponent()
        {
            var countryCode = PhoneCountryCode.Restore("+380");

            var component = Assert.Single(countryCode.GetEqualityComponents());

            Assert.Equal(
                "+380",
                component
            );

            Assert.IsType<string>(component);
        }

        [Fact]
        public void Equals_DifferentCodes_ReturnsFalse()
        {
            var first = PhoneCountryCode.Restore("+1");
            var second = PhoneCountryCode.Restore("+44");

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion

        #region Create and Restore
        [Fact]
        public void CreateThenRestore_TrimmedValue_PreservesEqualityAndHashCode()
        {
            var createResult = PhoneCountryCode.Create("\t +380 \r\n");

            Assert.True(createResult.IsSuccess);
            Assert.Empty(createResult.Errors);

            var created = createResult.Value;
            var restored = PhoneCountryCode.Restore(created.Value);

            Assert.Equal(
                "+380",
                created.Value
            );

            Assert.Equal(
                "+380",
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
