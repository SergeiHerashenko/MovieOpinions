using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersDeletion.ValueObjects
{
    public class DeletionReasonTests
    {
        private const string DefaultReason = "No deletion reason was provided.";

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Create_EmptyValue_ReturnsDefaultReason(string? rawReason)
        {
            var result = DeletionReason.Create(rawReason);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);

            Assert.Equal(
                DefaultReason,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(600)]
        [InlineData(601)]
        [InlineData(1000)]
        public void Create_LongWhitespaceOnlyValue_ReturnsDefaultReason(int length)
        {
            var rawReason = new string(' ', length);

            var result = DeletionReason.Create(rawReason);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);

            Assert.Equal(
                DefaultReason,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("Account closed by user")]
        [InlineData("Користувач видалив обліковий запис")]
        [InlineData("Delete #42!")]
        [InlineData("Reason 🙂")]
        public void Create_ValidText_PreservesValue(string value)
        {
            var result = DeletionReason.Create(value);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("  Account closed  ", "Account closed")]
        [InlineData("\tReason\t", "Reason")]
        [InlineData("\r\nReason\r\n", "Reason")]
        [InlineData("\u00A0Reason\u00A0", "Reason")]
        public void Create_SurroundingWhitespace_TrimsValue(
            string rawReason,
            string expected)
        {
            var result = DeletionReason.Create(rawReason);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                expected,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData("Delete  account")]
        [InlineData("Delete\taccount")]
        [InlineData("Delete\naccount")]
        public void Create_InternalWhitespace_PreservesValue(string value)
        {
            var result = DeletionReason.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(599)]
        [InlineData(600)]
        public void Create_ValidBoundaryLength_Succeeds(int length)
        {
            var value = new string('a', length);

            var result = DeletionReason.Create(value);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value.Value
            );
        }

        [Theory]
        [InlineData(601, false)]
        [InlineData(602, false)]
        [InlineData(1000, false)]
        [InlineData(601, true)]
        [InlineData(602, true)]
        [InlineData(1000, true)]
        public void Create_NormalizedTextTooLong_ReturnsTooLongFailure(
            int length,
            bool withSurroundingWhitespace)
        {
            var core = new string('a', length);

            var rawReason = withSurroundingWhitespace
                ? $"  {core}  "
                : core;

            var result = DeletionReason.Create(rawReason);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                "USER_DELETION.TOO_LONG_REASON",
                error.Code
            );

            Assert.Equal(
                ErrorType.Validation,
                error.ErrorType
            );

            Assert.Equal(
                "Deletion reason exceeds the maximum allowed length. Owner: 'DeletionReason'.",
                error.Message
            );
        }

        [Fact]
        public void Create_RawLengthAboveLimitButNormalizedLengthValid_Succeeds()
        {
            var core = new string('a', 600);
            var rawReason = $"  {core}  ";

            var result = DeletionReason.Create(rawReason);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);

            Assert.Equal(
                core,
                result.Value.Value
            );
        }

        [Fact]
        public void Create_SupplementaryCharacters_UsesUtf16Length()
        {
            var valid = new string('a', 598) + "🙂";
            var invalid = new string('a', 599) + "🙂";

            Assert.Equal(
                600,
                valid.Length
            );

            Assert.Equal(
                601,
                invalid.Length
            );

            var validResult = DeletionReason.Create(valid);
            var invalidResult = DeletionReason.Create(invalid);

            Assert.True(validResult.IsSuccess);

            Assert.Equal(
                valid,
                validResult.Value.Value
            );

            Assert.True(invalidResult.IsFailure);

            var error = Assert.Single(invalidResult.Errors);

            Assert.Equal(
                "USER_DELETION.TOO_LONG_REASON",
                error.Code
            );
        }

        [Theory]
        [InlineData("Account closed by user")]
        [InlineData("Користувач видалив обліковий запис")]
        [InlineData("Reason 🙂")]
        public void Restore_ValidText_PreservesValue(string value)
        {
            var reason = DeletionReason.Restore(value);

            Assert.Equal(
                value,
                reason.Value
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Restore_EmptyValue_ThrowsDomainDataInconsistencyException(string? value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeletionReason.Restore(value!)
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
                "DeletionReason",
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
        }

        [Theory]
        [InlineData(601)]
        [InlineData(1000)]
        public void Restore_LongWhitespaceOnlyValue_ThrowsEmptyException(int length)
        {
            var value = new string(' ', length);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeletionReason.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("  Account closed  ", "Account closed")]
        [InlineData("\tReason\t", "Reason")]
        [InlineData("\r\nReason\r\n", "Reason")]
        [InlineData("\u00A0Reason\u00A0", "Reason")]
        public void Restore_SurroundingWhitespace_TrimsValue(
            string value,
            string expected)
        {
            var reason = DeletionReason.Restore(value);

            Assert.Equal(
                expected,
                reason.Value
            );
        }

        [Theory]
        [InlineData("Delete  account")]
        [InlineData("Delete\taccount")]
        [InlineData("Delete\naccount")]
        public void Restore_InternalWhitespace_PreservesValue(string value)
        {
            var reason = DeletionReason.Restore(value);

            Assert.Equal(
                value,
                reason.Value
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(599)]
        [InlineData(600)]
        public void Restore_ValidBoundaryLength_Succeeds(int length)
        {
            var value = new string('a', length);

            var reason = DeletionReason.Restore(value);

            Assert.Equal(
                value,
                reason.Value
            );
        }

        [Theory]
        [InlineData(601, false)]
        [InlineData(602, false)]
        [InlineData(1000, false)]
        [InlineData(601, true)]
        [InlineData(602, true)]
        [InlineData(1000, true)]
        public void Restore_NormalizedTextTooLong_ThrowsDomainDataInconsistencyException(
            int length,
            bool withSurroundingWhitespace)
        {
            var core = new string('a', length);
            var value = withSurroundingWhitespace
                ? $"  {core}  "
                : core;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeletionReason.Restore(value)
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
                "DeletionReason",
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
        }

        [Fact]
        public void Restore_RawLengthAboveLimitButNormalizedLengthValid_Succeeds()
        {
            var core = new string('a', 600);
            var value = $"  {core}  ";

            var reason = DeletionReason.Restore(value);

            Assert.Equal(
                core,
                reason.Value
            );
        }

        [Fact]
        public void Restore_SupplementaryCharacters_UsesUtf16Length()
        {
            var valid = new string('a', 598) + "🙂";
            var invalid = new string('a', 599) + "🙂";

            Assert.Equal(
                600,
                valid.Length
            );

            Assert.Equal(
                601,
                invalid.Length
            );

            var restored = DeletionReason.Restore(valid);

            Assert.Equal(
                valid,
                restored.Value
            );

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeletionReason.Restore(invalid)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );
        }

        [Fact]
        public void GetEqualityComponents_NormalizedReason_ReturnsSingleStringComponent()
        {
            var result = DeletionReason.Create("  Account closed  ");

            Assert.True(result.IsSuccess);

            var component = Assert.Single(result.Value.GetEqualityComponents());

            Assert.Equal(
                "Account closed",
                component
            );

            Assert.IsType<string>(component);
        }

        [Theory]
        [InlineData("Reason one", "Reason two")]
        [InlineData("Reason", "reason")]
        [InlineData("A B", "A  B")]
        public void Equals_DifferentNormalizedValues_ReturnsFalse(
            string firstValue,
            string secondValue)
        {
            var first = DeletionReason.Create(firstValue);
            var second = DeletionReason.Create(secondValue);

            Assert.True(first.IsSuccess);
            Assert.True(second.IsSuccess);

            Assert.False(first.Value.Equals((object)second.Value));
            Assert.False(second.Value.Equals((object)first.Value));
        }

        [Theory]
        [InlineData("Account closed")]
        [InlineData("  Account closed  ")]
        [InlineData(null)]
        public void CreateThenRestore_PreservesValueEqualityAndHashCode(string? rawReason)
        {
            var createResult = DeletionReason.Create(rawReason);

            Assert.True(createResult.IsSuccess);

            var created = createResult.Value;
            var restored = DeletionReason.Restore(created.Value);

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
    }
}
