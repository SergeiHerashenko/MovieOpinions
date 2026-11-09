using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersRestrictionSession.ValueObjects
{
    public class UserRestrictionSessionIdTests
    {
        [Fact]
        public void Create_ReturnsNonEmptyVersion7Guid()
        {
            var id = UserRestrictionSessionId.Create();

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
            var first = UserRestrictionSessionId.Create();
            var second = UserRestrictionSessionId.Create();

            Assert.NotEqual(
                first.Value,
                second.Value
            );
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000001")]
        [InlineData("11111111-1111-4111-8111-111111111111")]
        [InlineData("0197f1a0-1234-7abc-8def-0123456789ab")]
        [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
        public void Restore_NonEmptyGuid_PreservesValue(string guidText)
        {
            var value = Guid.Parse(guidText);

            var id = UserRestrictionSessionId.Restore(value);

            Assert.Equal(
                value,
                id.Value
            );
        }

        [Fact]
        public void Restore_EmptyGuid_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserRestrictionSessionId.Restore(Guid.Empty)
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
                "UserRestrictionSessionId",
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
                "The field 'value' in type 'UserRestrictionSessionId' is empty!",
                exception.Message
            );
        }

        [Fact]
        public void ImplicitConversion_ValidId_ReturnsUnderlyingGuid()
        {
            var value = Guid.Parse("11111111-1111-4111-8111-111111111111");
            var id = UserRestrictionSessionId.Restore(value);

            Guid actual = id;

            Assert.Equal(
                value,
                actual
            );
        }

        [Fact]
        public void GetEqualityComponents_ValidId_ReturnsSingleGuidComponent()
        {
            var value = Guid.Parse("11111111-1111-4111-8111-111111111111");
            var id = UserRestrictionSessionId.Restore(value);

            var component = Assert.Single(id.GetEqualityComponents());

            Assert.Equal(
                value,
                component
            );

            Assert.IsType<Guid>(component);
        }

        [Fact]
        public void CreateThenRestore_PreservesValueEqualityAndHashCode()
        {
            var created = UserRestrictionSessionId.Create();
            var restored = UserRestrictionSessionId.Restore(created.Value);

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
            var first = UserRestrictionSessionId.Restore(
                Guid.Parse("11111111-1111-4111-8111-111111111111")
            );

            var second = UserRestrictionSessionId.Restore(
                Guid.Parse("22222222-2222-4222-8222-222222222222")
            );

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
    }
}
