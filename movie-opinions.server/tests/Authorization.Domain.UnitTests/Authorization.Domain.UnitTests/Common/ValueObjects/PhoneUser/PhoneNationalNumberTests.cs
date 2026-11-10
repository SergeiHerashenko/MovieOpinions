using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.PhoneUser;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.PhoneUser
{
    public class PhoneNationalNumberTests
    {
        #region Create
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Create_EmptyValue_ReturnsEmptyFailure(string? rawValue)
        {
            var result = PhoneNationalNumber.Create(rawValue!);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "NATIONAL_NUMBER.EMPTY_NATIONAL_NUMBER",
                error.Code
            );

            Assert.Equal(
                "National phone number is empty or missing. Owner: 'PhoneNationalNumber'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("1234")]
        [InlineData("0123")]
        [InlineData("0000")]
        [InlineData("0981234567")]
        [InlineData("1234567890123")]
        public void Create_ValidDigits_PreservesValue(string value)
        {
            var result = PhoneNationalNumber.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(12)]
        [InlineData(13)]
        public void Create_ValidDigitCount_Succeeds(int digitCount)
        {
            var value = new string('1', digitCount);

            var result = PhoneNationalNumber.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("12-34", "1234")]
        [InlineData("(123) 456-78-90", "1234567890")]
        [InlineData("(098) 123-45-67", "0981234567")]
        [InlineData("\t 1234 \r\n", "1234")]
        [InlineData("12\t34", "1234")]
        [InlineData("12\r\n34", "1234")]
        [InlineData("12\u00A034", "1234")]
        public void Create_FormattedNumber_NormalizesToDigits(
            string rawValue,
            string expected)
        {
            var result = PhoneNationalNumber.Create(rawValue);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                expected,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("12--34")]
        [InlineData("((1234")]
        [InlineData(")12(34-")]
        public void Create_AllowedFormattingInDifferentPositions_NormalizesToDigits(string rawValue)
        {
            var result = PhoneNationalNumber.Create(rawValue);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                "1234",
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("+1234")]
        [InlineData("12a34")]
        [InlineData("12.34")]
        [InlineData("12/34")]
        [InlineData("12_34")]
        [InlineData("12:34")]
        [InlineData("12🙂34")]
        [InlineData("12–34")]
        public void Create_ForbiddenCharacters_ReturnsInvalidFormatFailure(string value)
        {
            var result = PhoneNationalNumber.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "NATIONAL_NUMBER.INVALID_FORMAT_NATIONAL_NUMBER",
                error.Code
            );

            Assert.Equal(
                "National phone number contains unsupported characters. Only ASCII digits," +
                " whitespace characters, hyphens, and parentheses are allowed. Owner: 'PhoneNationalNumber'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("١٢٣٤")]
        [InlineData("۱۲۳۴")]
        [InlineData("１２３４")]
        [InlineData("12٣4")]
        public void Create_NonAsciiDigits_ReturnsInvalidFormatFailure(string value)
        {
            var result = PhoneNationalNumber.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "NATIONAL_NUMBER.INVALID_FORMAT_NATIONAL_NUMBER",
                error.Code
            );
        }

        [Theory]
        [InlineData("---")]
        [InlineData("()")]
        [InlineData("(-)")]
        [InlineData(" - () ")]
        public void Create_FormattingWithoutDigits_ReturnsTooShortFailure(string value)
        {
            var result = PhoneNationalNumber.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "NATIONAL_NUMBER.TOO_SHORT_NATIONAL_NUMBER",
                error.Code
            );

            Assert.Equal(
                "National phone number contains too few digits. Actual digit count: 0. " +
                "Minimum required: 4. Owner: 'PhoneNationalNumber'.",
                error.Message
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Create_TooFewDigits_ReturnsTooShortFailure(int digitCount)
        {
            var value = new string('1', digitCount);

            var result = PhoneNationalNumber.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "NATIONAL_NUMBER.TOO_SHORT_NATIONAL_NUMBER",
                error.Code
            );

            Assert.Equal(
                $"National phone number contains too few digits. " +
                $"Actual digit count: {digitCount}. Minimum required: 4. Owner: 'PhoneNationalNumber'.",
                error.Message
            );
        }

        [Theory]
        [InlineData(14)]
        [InlineData(15)]
        [InlineData(30)]
        public void Create_TooManyDigits_ReturnsTooLongFailure(int digitCount)
        {
            var value = new string('1', digitCount);

            var result = PhoneNationalNumber.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "NATIONAL_NUMBER.TOO_LONG_NATIONAL_NUMBER",
                error.Code
            );

            Assert.Equal(
                $"National phone number contains too many digits. " +
                $"Actual digit count: {digitCount}. Maximum allowed: 13. Owner: 'PhoneNationalNumber'.",
                error.Message
            );
        }

        // Межі стосуються кількості цифр після видалення дозволеного форматування.
        // The boundaries apply to digit count after removing allowed formatting.
        [Theory]
        [InlineData(4)]
        [InlineData(13)]
        public void Create_FormattedBoundaryNumber_UsesDigitCount(int digitCount)
        {
            var rawValue = " ( " + new string('1', digitCount) + " ) - - - ";
            var expected = new string('1', digitCount);

            var result = PhoneNationalNumber.Create(rawValue);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                expected,
                result.Value.Value
            );
        }

        [Fact]
        public void Create_FormattedTooShortNumber_ReportsDigitCount()
        {
            var result = PhoneNationalNumber.Create("(12)-3");

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "NATIONAL_NUMBER.TOO_SHORT_NATIONAL_NUMBER",
                error.Code
            );

            Assert.Equal(
                "National phone number contains too few digits. " +
                "Actual digit count: 3. Minimum required: 4. Owner: 'PhoneNationalNumber'.",
                error.Message
            );
        }

        [Fact]
        public void Create_FormattedTooLongNumber_ReportsDigitCount()
        {
            var rawValue = "(" + new string('1', 14) + ") - ";

            var result = PhoneNationalNumber.Create(rawValue);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "NATIONAL_NUMBER.TOO_LONG_NATIONAL_NUMBER",
                error.Code
            );

            Assert.Equal(
                "National phone number contains too many digits. " +
                "Actual digit count: 14. Maximum allowed: 13. Owner: 'PhoneNationalNumber'.",
                error.Message
            );
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12a3")]
        [InlineData("12345678901234a")]
        public void Create_InvalidFormatAndDigitCount_ReturnsFormatFailure(string value)
        {
            var result = PhoneNationalNumber.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "NATIONAL_NUMBER.INVALID_FORMAT_NATIONAL_NUMBER",
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
        [InlineData("\u00A0")]
        public void Restore_EmptyValue_ThrowsDomainDataInconsistencyException(string? storedValue)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneNationalNumber.Restore(storedValue!)
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
                "PhoneNationalNumber",
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
        [InlineData("1234")]
        [InlineData("0123")]
        [InlineData("0000")]
        [InlineData("0981234567")]
        [InlineData("1234567890123")]
        public void Restore_CanonicalNumber_PreservesValue(string value)
        {
            var nationalNumber = PhoneNationalNumber.Restore(value);

            Assert.Equal(
                value,
                nationalNumber.Value
            );
        }

        [Theory]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(12)]
        [InlineData(13)]
        public void Restore_CanonicalNumber_PreservesBoundaryDigitCount(int digitCount)
        {
            var value = new string('1', digitCount);

            var nationalNumber = PhoneNationalNumber.Restore(value);

            Assert.Equal(
                value,
                nationalNumber.Value
            );
        }

        [Theory]
        [InlineData("+1234")]
        [InlineData("12a34")]
        [InlineData("12.34")]
        [InlineData("12/34")]
        [InlineData("12_34")]
        [InlineData("12:34")]
        [InlineData("12🙂34")]
        [InlineData("12–34")]
        [InlineData("١٢٣٤")]
        [InlineData("۱۲۳۴")]
        [InlineData("１２３４")]
        [InlineData("12٣4")]
        public void Restore_ForbiddenCharacters_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneNationalNumber.Restore(value)
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
                "PhoneNationalNumber",
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
        [InlineData("12-34")]
        [InlineData("(123) 456-78-90")]
        [InlineData("(098) 123-45-67")]
        [InlineData("\t 1234 \r\n")]
        [InlineData("12\t34")]
        [InlineData("12\r\n34")]
        [InlineData("12\u00A034")]
        [InlineData("12--34")]
        [InlineData("((1234")]
        [InlineData(")12(34-")]
        public void Restore_FormattedNumber_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneNationalNumber.Restore(value)
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
                "PhoneNationalNumber",
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
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Restore_TooFewDigits_ThrowsDomainDataInconsistencyException(int digitCount)
        {
            var value = new string('1', digitCount);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneNationalNumber.Restore(value)
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
                "PhoneNationalNumber",
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
                digitCount,
                exception.Context["Custom_ActualDigitCount"]
            );

            Assert.Equal(
                4,
                exception.Context["Custom_MinimumDigitCount"]
            );
        }

        [Theory]
        [InlineData(14)]
        [InlineData(15)]
        [InlineData(30)]
        public void Restore_TooManyDigits_ThrowsDomainDataInconsistencyException(int digitCount)
        {
            var value = new string('1', digitCount);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneNationalNumber.Restore(value)
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
                "PhoneNationalNumber",
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
                digitCount,
                exception.Context["Custom_ActualDigitCount"]
            );

            Assert.Equal(
                13,
                exception.Context["Custom_MaximumDigitCount"]
            );
        }

        [Theory]
        [InlineData("---")]
        [InlineData("()")]
        [InlineData("(-)")]
        [InlineData(" - () ")]
        public void Restore_FormattingWithoutDigits_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneNationalNumber.Restore(value)
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
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                0,
                exception.Context["Custom_ActualDigitCount"]
            );

            Assert.Equal(
                4,
                exception.Context["Custom_MinimumDigitCount"]
            );
        }

        [Fact]
        public void Restore_FormattedTooShortNumber_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneNationalNumber.Restore("(12)-3")
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                3,
                exception.Context["Custom_ActualDigitCount"]
            );

            Assert.Equal(
                4,
                exception.Context["Custom_MinimumDigitCount"]
            );
        }

        [Fact]
        public void Restore_FormattedTooLongNumber_ThrowsDomainDataInconsistencyException()
        {
            var value = "(" + new string('1', 14) + ") - ";

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneNationalNumber.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                14,
                exception.Context["Custom_ActualDigitCount"]
            );

            Assert.Equal(
                13,
                exception.Context["Custom_MaximumDigitCount"]
            );
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12a3")]
        [InlineData("12345678901234a")]
        public void Restore_InvalidFormatAndDigitCount_ThrowsDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PhoneNationalNumber.Restore(value)
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
        public void GetEqualityComponents_ValidNumber_ReturnsSingleStringComponent()
        {
            var nationalNumber = PhoneNationalNumber.Restore("0981234567");

            var component = Assert.Single(nationalNumber.GetEqualityComponents());

            Assert.Equal(
                "0981234567",
                component
            );

            Assert.IsType<string>(component);
        }

        [Fact]
        public void Equals_DifferentNumbers_ReturnsFalse()
        {
            var first = PhoneNationalNumber.Restore("1234");
            var second = PhoneNationalNumber.Restore("5678");

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion

        #region Create and Restore
        [Fact]
        public void CreateThenRestore_FormattedNumber_PreservesEqualityAndHashCode()
        {
            var createResult = PhoneNationalNumber.Create("(098) 123-45-67");

            Assert.True(createResult.IsSuccess);
            Assert.Empty(createResult.Errors);

            var created = createResult.Value;
            var restored = PhoneNationalNumber.Restore(created.Value);

            Assert.Equal(
                "0981234567",
                created.Value
            );

            Assert.Equal(
                "0981234567",
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
