using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersPendingAction.ValueObjects
{
    public class ConfirmationFlowTokenTests
    {
        private static string Base64ForBytes(byte fillValue = 0, int byteCount = 64)
        {
            var bytes = new byte[byteCount];
            Array.Fill(bytes, fillValue);
            return Convert.ToBase64String(bytes);
        }

        [Fact]
        public void Create_GeneratesValidBase64Token()
        {
            var token = ConfirmationFlowToken.Create();

            Assert.False(string.IsNullOrWhiteSpace(token.Value));

            Assert.Equal(
                88,
                token.Value.Length
            );

            var decoded = Convert.FromBase64String(token.Value);

            Assert.Equal(
                64,
                decoded.Length
            );

            Assert.EndsWith(
                "==",
                token.Value
            );
        }

        [Fact]
        public void CreateThenParse_PreservesValueEqualityAndHashCode()
        {
            var created = ConfirmationFlowToken.Create();

            var result = ConfirmationFlowToken.Parse(created.Value);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);

            var parsed = result.Value;

            Assert.Equal(
                created.Value,
                parsed.Value
            );

            Assert.NotSame(
                created,
                parsed
            );

            Assert.True(created.Equals((object)parsed));
            Assert.True(parsed.Equals((object)created));

            Assert.Equal(
                created.GetHashCode(),
                parsed.GetHashCode()
            );
        }

        [Theory]
        [InlineData((byte)0)]
        [InlineData((byte)251)]
        [InlineData((byte)255)]
        public void Parse_ValidToken_PreservesExactValue(byte fillValue)
        {
            var value = Base64ForBytes(fillValue);

            var result = ConfirmationFlowToken.Parse(value);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Parse_EmptyValue_ReturnsEmptyFailure(string? value)
        {
            var result = ConfirmationFlowToken.Parse(value);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "CONFIRMATION_FLOW_TOKEN.EMPTY",
                error.Code
            );

            Assert.Equal(
                "Confirmation flow token validation failed " +
                "for type 'ConfirmationFlowToken': the value is empty!",
                error.Message
            );
        }

        [Fact]
        public void Parse_OnlyWhitespaceOfExpectedLength_ReturnsEmptyFailure()
        {
            var value = new string(' ', 88);

            var result = ConfirmationFlowToken.Parse(value);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "CONFIRMATION_FLOW_TOKEN.EMPTY",
                error.Code
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(87)]
        [InlineData(89)]
        [InlineData(176)]
        public void Parse_InvalidLength_ReturnsInvalidLengthFailure(int length)
        {
            var value = new string('A', length);

            var result = ConfirmationFlowToken.Parse(value);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "CONFIRMATION_FLOW_TOKEN.INVALID_LENGTH",
                error.Code
            );

            Assert.Equal(
                $"Confirmation flow token validation failed for type 'ConfirmationFlowToken': " +
                $"expected encoded length is 88, but the actual length is {length}.",
                error.Message
            );
        }

        [Theory]
        [InlineData(87)]
        [InlineData(89)]
        public void Parse_InvalidLengthAndFormat_ReturnsLengthFailure(int length)
        {
            var value = new string('!', length);

            var result = ConfirmationFlowToken.Parse(value);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "CONFIRMATION_FLOW_TOKEN.INVALID_LENGTH",
                error.Code
            );
        }

        [Theory]
        [InlineData('!')]
        [InlineData('@')]
        [InlineData('-')]
        [InlineData('_')]
        [InlineData('Ж')]
        public void Parse_InvalidCharactersAtExpectedLength_ReturnsInvalidFormatFailure(char invalidChar)
        {
            var value = Base64ForBytes();
            var chars = value.ToCharArray();
            chars[0] = invalidChar;
            var invalidValue = new string(chars);

            var result = ConfirmationFlowToken.Parse(invalidValue);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "CONFIRMATION_FLOW_TOKEN.INVALID_FORMAT",
                error.Code
            );

            Assert.Equal(
                "Confirmation flow token validation failed " +
                "for type 'ConfirmationFlowToken': the value is not valid Base64.",
                error.Message
            );
        }

        [Fact]
        public void Parse_InvalidPaddingAtExpectedLength_ReturnsInvalidFormatFailure()
        {
            var value = Base64ForBytes();

            var startsWithEquals = "=" + value[1..];
            var endsWithEqualsA = value[..^2] + "=A";
            var endsWithTripleEquals = value[..^3] + "===";

            Assert.Equal(
                88,
                startsWithEquals.Length
            );

            Assert.Equal(
                88,
                endsWithEqualsA.Length
            );

            Assert.Equal(
                88,
                endsWithTripleEquals.Length
            );

            AssertInvalidFormat(startsWithEquals);
            AssertInvalidFormat(endsWithEqualsA);
            AssertInvalidFormat(endsWithTripleEquals);
        }

        [Theory]
        [InlineData(65)]
        [InlineData(66)]
        public void Parse_Base64ForWrongByteCountWithExpectedLength_ReturnsInvalidFormatFailure(
            int byteCount)
        {
            var value = Base64ForBytes(byteCount: byteCount);

            Assert.Equal(
                88,
                value.Length
            );

            AssertInvalidFormat(value);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Parse_MissingPadding_ReturnsInvalidLengthFailure(
            int trimCount)
        {
            var value = Base64ForBytes();
            var trimmed = value[..^trimCount];

            var result = ConfirmationFlowToken.Parse(trimmed);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "CONFIRMATION_FLOW_TOKEN.INVALID_LENGTH",
                error.Code
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(8)]
        [InlineData(-1)]
        public void Parse_WhitespaceAroundOrInsideToken_ReturnsInvalidLengthFailure(int insertAt)
        {
            var value = Base64ForBytes();
            const string whitespace = " \t\r\n";

            var withWhitespace = insertAt switch
            {
                0 => whitespace + value,
                -1 => value + whitespace,
                _ => value.Insert(insertAt, whitespace)
            };

            var result = ConfirmationFlowToken.Parse(withWhitespace);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "CONFIRMATION_FLOW_TOKEN.INVALID_LENGTH",
                error.Code
            );
        }

        [Theory]
        [InlineData(' ')]
        [InlineData('\t')]
        public void Parse_WhitespaceReplacingCharacter_ReturnsInvalidFormatFailure(
            char whitespace)
        {
            var value = Base64ForBytes();
            var chars = value.ToCharArray();
            chars[10] = whitespace;
            var invalidValue = new string(chars);

            Assert.Equal(
                88,
                invalidValue.Length
            );

            AssertInvalidFormat(invalidValue);
        }

        [Fact]
        public void GetEqualityComponents_ValidToken_ReturnsSingleStringComponent()
        {
            var value = Base64ForBytes();
            var result = ConfirmationFlowToken.Parse(value);

            Assert.True(result.IsSuccess);

            var component = Assert.Single(result.Value.GetEqualityComponents());

            Assert.Equal(
                value,
                component
            );

            Assert.IsType<string>(component);
        }

        [Fact]
        public void Equals_DifferentTokenValues_ReturnsFalse()
        {
            var firstResult = ConfirmationFlowToken.Parse(Base64ForBytes(0));
            var secondResult = ConfirmationFlowToken.Parse(Base64ForBytes(255));

            Assert.True(firstResult.IsSuccess);
            Assert.True(secondResult.IsSuccess);

            Assert.False(firstResult.Value.Equals((object)secondResult.Value));
            Assert.False(secondResult.Value.Equals((object)firstResult.Value));
        }

        private static void AssertInvalidFormat(string value)
        {
            var result = ConfirmationFlowToken.Parse(value);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "CONFIRMATION_FLOW_TOKEN.INVALID_FORMAT",
                error.Code
            );

            Assert.Equal(
                "Confirmation flow token validation failed for " +
                "type 'ConfirmationFlowToken': the value is not valid Base64.",
                error.Message
            );
        }
    }
}
