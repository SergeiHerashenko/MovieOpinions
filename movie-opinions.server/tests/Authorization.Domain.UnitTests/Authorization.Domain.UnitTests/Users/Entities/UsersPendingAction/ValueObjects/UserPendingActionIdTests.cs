using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersPendingAction.ValueObjects
{
    public class UserPendingActionIdTests
    {
        #region Create
        [Fact]
        public void Create_ReturnsNonEmptyVersion7Guid()
        {
            var id = UserPendingActionId.Create();

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
            var first = UserPendingActionId.Create();
            var second = UserPendingActionId.Create();

            Assert.NotEqual(
                first.Value,
                second.Value
            );
        }
        #endregion

        #region Restore
        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000001")]
        [InlineData("11111111-1111-4111-8111-111111111111")]
        [InlineData("0197f1a0-1234-7abc-8def-0123456789ab")]
        [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
        public void Restore_NonEmptyGuid_PreservesValue(string guidText)
        {
            var value = Guid.Parse(guidText);

            var id = UserPendingActionId.Restore(value);

            Assert.Equal(
                value,
                id.Value
            );
        }

        [Fact]
        public void Restore_EmptyGuid_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserPendingActionId.Restore(Guid.Empty)
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
                "UserPendingActionId",
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
        #endregion

        #region Implicit conversion
        [Fact]
        public void ImplicitConversion_ValidId_ReturnsUnderlyingGuid()
        {
            var value = Guid.Parse("11111111-1111-4111-8111-111111111111");
            var id = UserPendingActionId.Restore(value);

            Guid actual = id;

            Assert.Equal(
                value,
                actual
            );
        }
        #endregion

        #region Equality
        [Fact]
        public void GetEqualityComponents_ValidId_ReturnsSingleGuidComponent()
        {
            var value = Guid.Parse("11111111-1111-4111-8111-111111111111");
            var id = UserPendingActionId.Restore(value);

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
            var created = UserPendingActionId.Create();
            var restored = UserPendingActionId.Restore(created.Value);

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
            var first = UserPendingActionId.Restore(
                Guid.Parse("11111111-1111-4111-8111-111111111111")
            );

            var second = UserPendingActionId.Restore(
                Guid.Parse("22222222-2222-4222-8222-222222222222")
            );

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion
    }
}
