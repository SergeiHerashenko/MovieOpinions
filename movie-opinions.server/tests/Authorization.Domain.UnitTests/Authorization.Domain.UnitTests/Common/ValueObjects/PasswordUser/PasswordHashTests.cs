using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.PasswordUser;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.PasswordUser
{
    public class PasswordHashTests
    {
        #region Create
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        [InlineData("\u00A0")]
        public void Create_EmptyValue_ThrowsDomainInvalidOperationException(string? value)
        {
            var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                PasswordHash.Create(value!)
            );

            Assert.Equal(
                "DOMAIN.INVALID_OPERATION.PRECONDITION_FAILED",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvalidOperation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "PasswordHash",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "Create",
                exception.Context["SourceContext"]
            );

            Assert.Equal(
                "A non-empty password hash.",
                exception.Context["Prerequisite"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Custom_FieldName"]
            );

            Assert.Equal(
                "Precondition failed in 'Create' for type 'PasswordHash'. " +
                "Required prerequisite 'A non-empty password hash.' was not satisfied.",
                exception.Message
            );
        }

        [Theory]
        [InlineData("TEST.HASH")]
        [InlineData("A")]
        [InlineData("0")]
        [InlineData("AbCdEf0123+/=")]
        public void Create_NonEmptyValue_PreservesValue(string value)
        {
            var hash = PasswordHash.Create(value);

            Assert.Equal(
                value,
                hash.Value
            );
        }

        // Хеш є непрозорим значенням: пробіли не обрізаються.
        // The hash is an opaque value: whitespace is not trimmed.
        [Theory]
        [InlineData(" TEST.HASH")]
        [InlineData("TEST.HASH ")]
        [InlineData("  TEST.HASH  ")]
        [InlineData("\tTEST.HASH\r\n")]
        public void Create_SurroundingWhitespace_PreservesValue(string value)
        {
            var hash = PasswordHash.Create(value);

            Assert.Equal(
                value,
                hash.Value
            );
        }

        [Theory]
        [InlineData(64)]
        [InlineData(65)]
        [InlineData(256)]
        public void Create_LongValue_PreservesValue(int length)
        {
            var value = new string('H', length);

            var hash = PasswordHash.Create(value);

            Assert.Equal(
                value,
                hash.Value
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
        public void Restore_EmptyValue_ThrowsDomainDataInconsistencyException(
            string? value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                PasswordHash.Restore(value!)
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
                "PasswordHash",
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
                "An error occurred during the 'Restore' operation. " +
                "The field 'Value' in type 'PasswordHash' is empty!",
                exception.Message
            );
        }

        [Theory]
        [InlineData("TEST.HASH")]
        [InlineData("A")]
        [InlineData("0")]
        [InlineData("AbCdEf0123+/=")]
        public void Restore_NonEmptyValue_PreservesValue(string value)
        {
            var hash = PasswordHash.Restore(value);

            Assert.Equal(
                value,
                hash.Value
            );
        }

        [Theory]
        [InlineData(" TEST.HASH")]
        [InlineData("TEST.HASH ")]
        [InlineData("  TEST.HASH  ")]
        [InlineData("\tTEST.HASH\r\n")]
        public void Restore_SurroundingWhitespace_PreservesValue(string value)
        {
            var hash = PasswordHash.Restore(value);

            Assert.Equal(
                value,
                hash.Value
            );
        }

        [Theory]
        [InlineData(64)]
        [InlineData(65)]
        [InlineData(256)]
        public void Restore_LongValue_PreservesValue(int length)
        {
            var value = new string('H', length);

            var hash = PasswordHash.Restore(value);

            Assert.Equal(
                value,
                hash.Value
            );
        }
        #endregion

        #region Equality and hash code
        [Fact]
        public void GetEqualityComponents_ValidHash_ReturnsSingleStringComponent()
        {
            var hash = PasswordHash.Create("TEST.HASH");

            var component = Assert.Single(hash.GetEqualityComponents());

            Assert.Equal(
                "TEST.HASH",
                component
            );

            Assert.IsType<string>(component);
        }

        [Theory]
        [InlineData("TEST.HASH.1", "TEST.HASH.2")]
        [InlineData("TEST.HASH", "test.hash")]
        [InlineData("TEST.HASH", " TEST.HASH ")]
        public void Equals_DifferentHashValues_ReturnsFalse(
            string firstValue,
            string secondValue)
        {
            var first = PasswordHash.Create(firstValue);
            var second = PasswordHash.Create(secondValue);

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion

        #region Create and Restore
        [Fact]
        public void CreateThenRestore_PreservesValueEqualityAndHashCode()
        {
            const string value = " AbCdEf0123+/= ";

            var created = PasswordHash.Create(value);
            var restored = PasswordHash.Restore(created.Value);

            Assert.Equal(
                value,
                created.Value
            );

            Assert.Equal(
                value,
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
