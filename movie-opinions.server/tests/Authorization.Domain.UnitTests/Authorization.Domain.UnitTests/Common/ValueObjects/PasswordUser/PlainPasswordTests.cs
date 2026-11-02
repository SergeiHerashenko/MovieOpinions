using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.ValueObjects.PasswordUser;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.PasswordUser
{
    public class PlainPasswordTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Create_EmptyValue_ReturnsEmptyFailure(string? rawValue)
        {
            var result = PlainPassword.Create(rawValue!);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PLAIN_PASSWORD.EMPTY_PLAIN_PASSWORD",
                error.Code
            );

            Assert.Equal(
                "Plain password is empty, missing, or contains only whitespace. Owner: 'PlainPassword'!",
                error.Message
            );
        }

        [Theory]
        [InlineData("Abcdefg1")]
        [InlineData("ABCdef12")]
        [InlineData("Aa1_____")]
        [InlineData("Password123!")]
        public void Create_ValidPassword_PreservesValue(string value)
        {
            var result = PlainPassword.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(63)]
        [InlineData(64)]
        public void Create_ValidBoundaryLength_Succeeds(int length)
        {
            var password = "Aa1" + new string('b', length - 3);

            var result = PlainPassword.Create(password);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                password,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(7)]
        public void Create_TooShortPassword_ReturnsTooShortFailure(int length)
        {
            var password = "Aa1" + new string('b', length - 3);

            var result = PlainPassword.Create(password);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PLAIN_PASSWORD.TOO_SHORT_PLAIN_PASSWORD",
                error.Code
            );

            Assert.Equal(
                $"Plain password length is below the required minimum. " +
                $"Actual length: '{length}'. Minimum length: '8'. Owner: 'PlainPassword'!",
                error.Message
            );
        }

        [Theory]
        [InlineData(65)]
        [InlineData(66)]
        [InlineData(100)]
        public void Create_TooLongPassword_ReturnsTooLongFailure(int length)
        {
            var password = "Aa1" + new string('b', length - 3);

            var result = PlainPassword.Create(password);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PLAIN_PASSWORD.TOO_LONG_PLAIN_PASSWORD",
                error.Code
            );

            Assert.Equal(
                $"Plain password length exceeds the allowed maximum. " +
                $"Actual length: '{length}'. Maximum length: '64'. Owner: 'PlainPassword'!",
                error.Message
            );
        }

        [Fact]
        public void Create_MissingLowercase_ReturnsMissingLowercaseFailure()
        {
            var result = PlainPassword.Create("ABCDEFG1");

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PLAIN_PASSWORD.MISSING_LOWERCASE_LETTER_PLAIN_PASSWORD",
                error.Code
            );

            Assert.Equal(
                "Plain password must contain at least one lowercase letter. Owner: 'PlainPassword'!",
                error.Message
            );
        }

        [Fact]
        public void Create_MissingUppercase_ReturnsMissingUppercaseFailure()
        {
            var result = PlainPassword.Create("abcdefg1");

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PLAIN_PASSWORD.MISSING_UPPERCASE_LETTER_PLAIN_PASSWORD",
                error.Code
            );

            Assert.Equal(
                "Plain password must contain at least one uppercase letter. Owner: 'PlainPassword'!",
                error.Message
            );
        }

        [Fact]
        public void Create_MissingAsciiDigit_ReturnsMissingNumberFailure()
        {
            var result = PlainPassword.Create("Abcdefgh");

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PLAIN_PASSWORD.MISSING_NUMBER_PLAIN_PASSWORD",
                error.Code
            );

            Assert.Equal(
                "Plain password must contain at least one ASCII digit from '0' to '9'. Owner: 'PlainPassword'!",
                error.Message
            );
        }

        [Theory]
        [InlineData("Aa١٢٣٤٥٦")]
        [InlineData("Aa۱۲۳۴۵۶")]
        [InlineData("Aa１２３４５６")]
        public void Create_NonAsciiDigitsWithoutAsciiDigit_ReturnsMissingNumberFailure(string value)
        {
            var result = PlainPassword.Create(value);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PLAIN_PASSWORD.MISSING_NUMBER_PLAIN_PASSWORD",
                error.Code
            );
        }

        [Theory]
        [InlineData("Пароль12")]
        [InlineData("Äbcdefg1")]
        [InlineData("äBCDEFG1")]
        public void Create_UnicodeLetters_SatisfiesLetterRequirements(string value)
        {
            var result = PlainPassword.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Fact]
        public void Create_NonAsciiDigitsAlongsideAsciiDigit_Succeeds()
        {
            const string value = "Aa١٢٣٤٥1";

            var result = PlainPassword.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("Abcdefg1!")]
        [InlineData("Abcdefg1🙂")]
        [InlineData("Aa1_-/()")]
        public void Create_AdditionalSymbols_PreservesValue(string value)
        {
            var result = PlainPassword.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(" Abcdefg1")]
        [InlineData("Abcdefg1 ")]
        [InlineData("  Abcdefg1  ")]
        [InlineData("\tAbcdefg1\r\n")]
        public void Create_SurroundingWhitespace_PreservesValue(string value)
        {
            var result = PlainPassword.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("Ab1 cdef")]
        [InlineData("Ab1\tcdef")]
        [InlineData("Ab1\ncdef")]
        public void Create_InternalWhitespace_PreservesValue(string value)
        {
            var result = PlainPassword.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Fact]
        public void Create_WhitespaceCountsTowardLength()
        {
            var password = "Aa1" + new string(' ', 5);

            var result = PlainPassword.Create(password);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                password,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("Abcdefé1")]
        [InlineData("Abcdefe\u03011")]
        public void Create_UnicodeForms_PreservesOriginalValue(string value)
        {
            var result = PlainPassword.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Fact]
        public void Create_SupplementaryCharacterAtMaximumLength_Succeeds()
        {
            var password = "Aa1" + new string('b', 59) + "🙂";

            var result = PlainPassword.Create(password);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                password,
                result.Value.Value
            );

            Assert.Equal(
                64,
                password.Length
            );
        }

        [Fact]
        public void Create_SupplementaryCharacterBeyondMaximumLength_ReturnsTooLongFailure()
        {
            var password = "Aa1" + new string('b', 60) + "🙂";

            var result = PlainPassword.Create(password);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PLAIN_PASSWORD.TOO_LONG_PLAIN_PASSWORD",
                error.Code
            );

            Assert.Equal(
                "Plain password length exceeds the allowed maximum. " +
                "Actual length: '65'. Maximum length: '64'. Owner: 'PlainPassword'!",
                error.Message
            );
        }

        [Theory]
        [InlineData("12345678", "PLAIN_PASSWORD.MISSING_LOWERCASE_LETTER_PLAIN_PASSWORD")]
        [InlineData("ABCDEFGH", "PLAIN_PASSWORD.MISSING_LOWERCASE_LETTER_PLAIN_PASSWORD")]
        [InlineData("abcdefgh", "PLAIN_PASSWORD.MISSING_UPPERCASE_LETTER_PLAIN_PASSWORD")]
        [InlineData("!!!!!!!!", "PLAIN_PASSWORD.MISSING_LOWERCASE_LETTER_PLAIN_PASSWORD")]
        public void Create_MultipleMissingRequirements_ReturnsFirstFailure(
            string value,
            string expectedCode)
        {
            var result = PlainPassword.Create(value);

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

        [Theory]
        [InlineData("A1", "PLAIN_PASSWORD.MISSING_LOWERCASE_LETTER_PLAIN_PASSWORD")]
        [InlineData("a1", "PLAIN_PASSWORD.MISSING_UPPERCASE_LETTER_PLAIN_PASSWORD")]
        [InlineData("Aa", "PLAIN_PASSWORD.MISSING_NUMBER_PLAIN_PASSWORD")]
        public void Create_MissingRequirementAndTooShort_ReturnsFormatFailure(
            string value,
            string expectedCode)
        {
            var result = PlainPassword.Create(value);

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
        public void Create_MissingDigitAndTooLong_ReturnsMissingNumberFailure()
        {
            var password = "Aa" + new string('a', 63);

            var result = PlainPassword.Create(password);

            Assert.True(result.IsFailure);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "PLAIN_PASSWORD.MISSING_NUMBER_PLAIN_PASSWORD",
                error.Code
            );
        }

        [Fact]
        public void GetEqualityComponents_ValidPassword_ReturnsSingleStringComponent()
        {
            var result = PlainPassword.Create("Abcdefg1");

            Assert.True(result.IsSuccess);

            var component = Assert.Single(result.Value.GetEqualityComponents());

            Assert.Equal(
                "Abcdefg1",
                component
            );

            Assert.IsType<string>(component);
        }

        [Theory]
        [InlineData("Abcdefg1", "Abcdefg2")]
        [InlineData("Abcdefg1", "AbcdefG1")]
        [InlineData("Abcdefg1", " Abcdefg1 ")]
        [InlineData("Abcdefé1", "Abcdefe\u03011")]
        public void Equals_DifferentPasswords_ReturnsFalse(
            string firstValue,
            string secondValue)
        {
            var firstResult = PlainPassword.Create(firstValue);
            var secondResult = PlainPassword.Create(secondValue);

            Assert.True(firstResult.IsSuccess);
            Assert.True(secondResult.IsSuccess);

            Assert.False(firstResult.Value.Equals((object)secondResult.Value));
            Assert.False(secondResult.Value.Equals((object)firstResult.Value));
        }

        [Fact]
        public void Create_SamePasswordTwice_ReturnsEqualObjectsWithSameHashCode()
        {
            var firstResult = PlainPassword.Create("Abcdefg1");
            var secondResult = PlainPassword.Create("Abcdefg1");

            Assert.True(firstResult.IsSuccess);
            Assert.True(secondResult.IsSuccess);
            Assert.Empty(firstResult.Errors);
            Assert.Empty(secondResult.Errors);

            var first = firstResult.Value;
            var second = secondResult.Value;

            Assert.Equal(
                "Abcdefg1",
                first.Value
            );

            Assert.Equal(
                "Abcdefg1",
                second.Value
            );

            Assert.NotSame(
                first,
                second
            );

            Assert.True(first.Equals((object)second));
            Assert.True(second.Equals((object)first));

            Assert.Equal(
                first.GetHashCode(),
                second.GetHashCode()
            );
        }
    }
}
