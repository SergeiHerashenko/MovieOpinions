using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersPendingAction.Actions
{
    public class ChangePasswordActionTests
    {
        private static Password CreatePassword(string hashValue)
            => Password.Create(PasswordHash.Create(hashValue));

        private static void AssertEmptyField(
            DomainDataInconsistencyException exception,
            string field,
            string operation)
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
                "ChangePasswordAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                field,
                exception.Context["Field"]
            );

            Assert.Equal(
                operation,
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("TEST.HASH")]
        [InlineData("AbCdEf0123+/=")]
        [InlineData(" TEST.HASH ")]
        public void Create_ValidPassword_PreservesPasswordAndSetsProperties(string hashValue)
        {
            var password = CreatePassword(hashValue);

            var action = ChangePasswordAction.Create(password);

            Assert.Same(
                password,
                action.NewPassword
            );

            Assert.Equal(
                UserActionType.ChangePassword,
                action.ActionType
            );

            Assert.Equal(
                hashValue,
                action.Value
            );
        }

        [Fact]
        public void Create_NullPassword_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                ChangePasswordAction.Create(null!)
            );

            AssertEmptyField(
                exception,
                "passwordHash",
                "Create"
            );
        }

        [Theory]
        [InlineData("TEST.HASH")]
        [InlineData("AbCdEf0123+/=")]
        [InlineData(" TEST.HASH ")]
        public void Restore_ValidHash_CreatesPasswordAndSetsProperties(string hashValue)
        {
            var hash = PasswordHash.Restore(hashValue);

            var action = ChangePasswordAction.Restore(hash);

            Assert.NotNull(action.NewPassword);

            Assert.Equal(
                hashValue,
                action.NewPassword.Value
            );

            Assert.Equal(
                UserActionType.ChangePassword,
                action.ActionType
            );

            Assert.Equal(
                hashValue,
                action.Value
            );
        }

        [Fact]
        public void Restore_NullHash_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                ChangePasswordAction.Restore(null!)
            );

            AssertEmptyField(
                exception,
                "hashPassword",
                "Restore"
            );
        }

        [Fact]
        public void GetEqualityComponents_ValidAction_ReturnsSinglePasswordComponent()
        {
            var password = CreatePassword("TEST.HASH");
            var action = ChangePasswordAction.Create(password);

            var component = Assert.Single(action.GetEqualityComponents());

            Assert.Same(
                password,
                component
            );
        }

        [Theory]
        [InlineData("TEST.HASH")]
        [InlineData("AbCdEf0123+/=")]
        [InlineData(" TEST.HASH ")]
        public void CreateThenRestore_EquivalentHashes_PreservesEqualityAndHashCode(
            string hashValue)
        {
            var firstHash = PasswordHash.Create(hashValue);
            var secondHash = PasswordHash.Restore(hashValue);

            var created = ChangePasswordAction.Create(
                Password.Create(firstHash)
            );

            var restored = ChangePasswordAction.Restore(secondHash);

            Assert.NotSame(
                created,
                restored
            );

            Assert.NotSame(
                created.NewPassword,
                restored.NewPassword
            );

            Assert.Equal(
                hashValue,
                created.Value
            );

            Assert.Equal(
                hashValue,
                restored.Value
            );

            Assert.True(created.Equals((object)restored));
            Assert.True(restored.Equals((object)created));

            Assert.Equal(
                created.GetHashCode(),
                restored.GetHashCode()
            );
        }

        [Theory]
        [InlineData("TEST.HASH.1", "TEST.HASH.2")]
        [InlineData("TEST.HASH", "test.hash")]
        [InlineData("TEST.HASH", " TEST.HASH ")]
        public void Equals_DifferentHashValues_ReturnsFalse(
            string firstHash,
            string secondHash)
        {
            var first = ChangePasswordAction.Create(CreatePassword(firstHash));
            var second = ChangePasswordAction.Create(CreatePassword(secondHash));

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
    }
}
