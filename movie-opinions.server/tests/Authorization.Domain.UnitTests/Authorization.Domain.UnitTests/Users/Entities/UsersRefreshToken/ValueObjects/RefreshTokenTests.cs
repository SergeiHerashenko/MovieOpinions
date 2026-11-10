using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersRefreshToken.ValueObjects
{
    public class RefreshTokenTests
    {
        #region Helpers: test data
        // Фіксовані байти ізолюють перевірки формату від генерації випадкового токена.
        // Fixed bytes isolate format checks from random token generation.
        private static string Base64ForBytes(
            byte fillValue = 0,
            int byteCount = 64)
        {
            var bytes = new byte[byteCount];
            Array.Fill(bytes, fillValue);
            return Convert.ToBase64String(bytes);
        }
        #endregion

        #region Helpers: assertions
        private static void AssertEmptyValue(DomainDataInconsistencyException exception)
        {
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
                "RefreshToken",
                exception.Context["Type"]
            );

            Assert.Equal(
                "value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "An error occurred during the 'Restore' operation. " +
                "The field 'value' in type 'RefreshToken' is empty!",
                exception.Message
            );
        }

        private static void AssertInvalidLength(
            DomainDataInconsistencyException exception,
            int actualLength)
        {
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
                "RefreshToken",
                exception.Context["Type"]
            );

            Assert.Equal(
                "value",
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
                actualLength,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                88,
                exception.Context["Custom_ExpectedLength"]
            );

            Assert.Equal(
                "An error occurred during the 'Restore' operation. " +
                "The field 'value' in type 'RefreshToken' has a value outside the allowed range!",
                exception.Message
            );
        }

        private static void AssertInvalidFormat(DomainDataInconsistencyException exception)
        {
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
                "RefreshToken",
                exception.Context["Type"]
            );

            Assert.Equal(
                "value",
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

            Assert.Equal(
                "An error occurred during the 'Restore' operation. " +
                "The field 'value' in type 'RefreshToken' has an invalid format!",
                exception.Message
            );
        }
        #endregion

        #region Create
        [Fact]
        public void Create_GeneratesValidBase64Token()
        {
            var token = RefreshToken.Create();

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
        public void Create_SeparateCalls_ReturnDifferentValues()
        {
            var first = RefreshToken.Create();
            var second = RefreshToken.Create();

            Assert.NotEqual(
                first.Value,
                second.Value
            );
        }

        [Fact]
        public void CreateThenRestore_PreservesValueEqualityAndHashCode()
        {
            var created = RefreshToken.Create();
            var restored = RefreshToken.Restore(created.Value);

            Assert.Equal(
                created.Value,
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

        #region Restore: valid tokens
        [Theory]
        [InlineData((byte)0)]
        [InlineData((byte)251)]
        [InlineData((byte)255)]
        public void Restore_ValidToken_PreservesExactValue(byte fillValue)
        {
            var value = Base64ForBytes(fillValue);

            var token = RefreshToken.Restore(value);

            Assert.Equal(
                value,
                token.Value
            );
        }
        #endregion

        #region Restore: required value
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Restore_EmptyValue_ThrowsDataInconsistency(string? value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RefreshToken.Restore(value!)
            );

            AssertEmptyValue(exception);
        }

        [Theory]
        [InlineData(87)]
        [InlineData(88)]
        [InlineData(89)]
        public void Restore_OnlyWhitespaceAtBoundaryLengths_ThrowsEmptyValue(int length)
        {
            var value = new string(' ', length);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RefreshToken.Restore(value)
            );

            AssertEmptyValue(exception);
        }
        #endregion

        #region Restore: length validation
        [Theory]
        [InlineData(1)]
        [InlineData(87)]
        [InlineData(89)]
        [InlineData(176)]
        public void Restore_InvalidLength_ThrowsDataInconsistency(int length)
        {
            var value = new string('A', length);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RefreshToken.Restore(value)
            );

            AssertInvalidLength(
                exception,
                length
            );
        }

        // За одночасних порушень довжина перевіряється раніше за формат.
        // When both rules fail, length validation takes precedence over format.
        [Theory]
        [InlineData(87)]
        [InlineData(89)]
        public void Restore_InvalidLengthAndFormat_ReportsLengthError(int length)
        {
            var value = new string('!', length);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RefreshToken.Restore(value)
            );

            AssertInvalidLength(
                exception,
                length
            );
        }
        #endregion

        #region Restore: Base64 format
        [Theory]
        [InlineData('!')]
        [InlineData('@')]
        [InlineData('-')]
        [InlineData('_')]
        [InlineData('Ж')]
        public void Restore_InvalidCharactersAtExpectedLength_ThrowsInvalidFormat(char invalidChar)
        {
            var value = Base64ForBytes();
            var chars = value.ToCharArray();
            chars[0] = invalidChar;
            var invalidValue = new string(chars);

            Assert.Equal(
                88,
                invalidValue.Length
            );

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RefreshToken.Restore(invalidValue)
            );

            AssertInvalidFormat(exception);
        }

        [Fact]
        public void Restore_InvalidPaddingAtExpectedLength_ThrowsInvalidFormat()
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

            AssertInvalidFormat(
                Assert.Throws<DomainDataInconsistencyException>(() =>
                    RefreshToken.Restore(startsWithEquals)
                )
            );

            AssertInvalidFormat(
                Assert.Throws<DomainDataInconsistencyException>(() =>
                    RefreshToken.Restore(endsWithEqualsA)
                )
            );

            AssertInvalidFormat(
                Assert.Throws<DomainDataInconsistencyException>(() =>
                    RefreshToken.Restore(endsWithTripleEquals)
                )
            );
        }

        // Довжина рядка може бути правильною, хоча токен кодує зайві байти.
        // The string length can be correct while the token encodes too many bytes.
        [Theory]
        [InlineData(65)]
        [InlineData(66)]
        public void Restore_Base64ForTooManyBytesWithExpectedLength_ThrowsInvalidFormat(int byteCount)
        {
            var value = Base64ForBytes(byteCount: byteCount);

            Assert.Equal(
                88,
                value.Length
            );

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RefreshToken.Restore(value)
            );

            AssertInvalidFormat(exception);
        }

        [Theory]
        [InlineData(61)]
        [InlineData(62)]
        [InlineData(63)]
        public void Restore_Base64ForTooFewBytesPaddedWithWhitespace_ThrowsInvalidFormat(int byteCount)
        {
            var value = Base64ForBytes(byteCount: byteCount) + " \t\r\n";

            Assert.Equal(
                88,
                value.Length
            );

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RefreshToken.Restore(value)
            );

            AssertInvalidFormat(exception);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Restore_MissingPadding_ReportsLengthError(int trimCount)
        {
            var value = Base64ForBytes();
            var trimmed = value[..^trimCount];

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RefreshToken.Restore(trimmed)
            );

            AssertInvalidLength(exception, trimmed.Length);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(8)]
        [InlineData(-1)]
        public void Restore_WhitespaceAroundOrInsideToken_ReportsLengthError(int insertAt)
        {
            var value = Base64ForBytes();
            const string whitespace = " \t\r\n";

            var withWhitespace = insertAt switch
            {
                0 => whitespace + value,
                -1 => value + whitespace,
                _ => value.Insert(insertAt, whitespace)
            };

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RefreshToken.Restore(withWhitespace)
            );

            AssertInvalidLength(
                exception,
                92
            );
        }

        [Theory]
        [InlineData(' ')]
        [InlineData('\t')]
        public void Restore_WhitespaceReplacingCharacter_ThrowsInvalidFormat(char whitespace)
        {
            var value = Base64ForBytes();
            var chars = value.ToCharArray();
            chars[10] = whitespace;
            var invalidValue = new string(chars);

            Assert.Equal(
                88,
                invalidValue.Length
            );

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RefreshToken.Restore(invalidValue)
            );

            AssertInvalidFormat(exception);
        }
        #endregion

        #region Equality
        [Fact]
        public void GetEqualityComponents_ValidToken_ReturnsSingleStringComponent()
        {
            var value = Base64ForBytes();
            var token = RefreshToken.Restore(value);

            var component = Assert.Single(token.GetEqualityComponents());

            Assert.Equal(
                value,
                component
            );

            Assert.IsType<string>(component);
        }

        [Fact]
        public void Equals_DifferentTokenValues_ReturnsFalse()
        {
            var first = RefreshToken.Restore(Base64ForBytes(0));
            var second = RefreshToken.Restore(Base64ForBytes(255));

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion
    }
}
