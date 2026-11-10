using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersRestriction.ValueObjects
{
    public class UserRestrictionIdTests
    {
        #region Create
        [Fact]
        public void Create_ReturnsNonEmptyVersion7Guid()
        {
            var id = UserRestrictionId.Create();

            Assert.NotEqual(
                Guid.Empty,
                id.Value
            );

            Assert.Equal(
                7,
                id.Value.Version
            );
        }

        [Fact]
        public void Create_SeparateCalls_ReturnDifferentValues()
        {
            var first = UserRestrictionId.Create();
            var second = UserRestrictionId.Create();

            Assert.NotEqual(
                first.Value,
                second.Value
            );
        }
        #endregion

        #region Restore
        // Відновлення приймає непорожні GUID різних версій, а не лише згенеровані v7.
        // Restoration accepts nonempty GUIDs of different versions, beyond generated v7 values.
        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000001")]
        [InlineData("11111111-1111-4111-8111-111111111111")]
        [InlineData("0197f1a0-1234-7abc-8def-0123456789ab")]
        [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
        public void Restore_NonEmptyGuid_PreservesValue(string guidText)
        {
            var value = Guid.Parse(guidText);

            var id = UserRestrictionId.Restore(value);

            Assert.Equal(
                value,
                id.Value
            );
        }

        [Fact]
        public void Restore_EmptyGuid_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionId.Restore(Guid.Empty)
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
                "UserRestrictionId",
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
                "The field 'value' in type 'UserRestrictionId' is empty!",
                exception.Message
            );
        }
        #endregion

        #region Implicit conversion
        [Fact]
        public void ImplicitConversion_ValidId_ReturnsUnderlyingGuid()
        {
            var value = Guid.Parse("11111111-1111-4111-8111-111111111111");
            var id = UserRestrictionId.Restore(value);

            Guid actual = id;

            Assert.Equal(
                value,
                actual
            );
        }
        #endregion

        #region GetEqualityComponents
        [Fact]
        public void GetEqualityComponents_ValidId_ReturnsSingleGuidComponent()
        {
            var value = Guid.Parse("11111111-1111-4111-8111-111111111111");
            var id = UserRestrictionId.Restore(value);

            var component = Assert.Single(id.GetEqualityComponents());

            Assert.Equal(
                value,
                component
            );

            Assert.IsType<Guid>(component);
        }
        #endregion

        #region Equality and hash code
        [Fact]
        public void CreateThenRestore_PreservesValueEqualityAndHashCode()
        {
            var created = UserRestrictionId.Create();
            var restored = UserRestrictionId.Restore(created.Value);

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

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var first = UserRestrictionId.Restore(
                Guid.Parse("11111111-1111-4111-8111-111111111111")
            );

            var second = UserRestrictionId.Restore(
                Guid.Parse("22222222-2222-4222-8222-222222222222")
            );

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion
    }
}
