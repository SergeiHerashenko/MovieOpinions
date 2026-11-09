using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersPendingAction.Actions
{
    public class DeleteUserActionTests
    {
        private static DeletionReason CreateReason(string? rawReason)
        {
            var result = DeletionReason.Create(rawReason);
            Assert.True(result.IsSuccess);
            return result.Value;
        }

        private static void AssertEmptyField(
            DomainDataInconsistencyException exception,
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
                "DeleteUserAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "reason",
                exception.Context["Field"]
            );

            Assert.Equal(
                operation,
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("Account closed", "Account closed")]
        [InlineData("Причина видалення", "Причина видалення")]
        [InlineData(null, "No deletion reason was provided.")]
        public void Create_ValidReason_PreservesReasonAndSetsProperties(
            string? rawReason,
            string expectedValue)
        {
            var reason = CreateReason(rawReason);

            var action = DeleteUserAction.Create(reason);

            Assert.Same(
                reason,
                action.Reason
            );

            Assert.Equal(
                UserActionType.DeleteUser,
                action.ActionType
            );

            Assert.Equal(
                expectedValue,
                action.Value
            );
        }

        [Fact]
        public void Create_NullReason_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeleteUserAction.Create(null!)
            );

            AssertEmptyField(
                exception,
                "Create"
            );
        }

        [Theory]
        [InlineData("Account closed", "Account closed")]
        [InlineData("Причина видалення", "Причина видалення")]
        [InlineData(null, "No deletion reason was provided.")]
        public void Restore_ValidReason_PreservesReasonAndSetsProperties(
            string? rawReason,
            string expectedValue)
        {
            var reason = CreateReason(rawReason);

            var action = DeleteUserAction.Restore(reason);

            Assert.Same(
                reason,
                action.Reason
            );

            Assert.Equal(
                UserActionType.DeleteUser,
                action.ActionType
            );

            Assert.Equal(
                expectedValue,
                action.Value
            );
        }

        [Fact]
        public void Restore_NullReason_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DeleteUserAction.Restore(null!)
            );

            AssertEmptyField(
                exception,
                "Restore"
            );
        }

        [Fact]
        public void GetEqualityComponents_ValidAction_ReturnsSingleReasonComponent()
        {
            var reason = CreateReason("Account closed");
            var action = DeleteUserAction.Create(reason);

            var component = Assert.Single(action.GetEqualityComponents());

            Assert.Same(
                reason,
                component
            );
        }

        [Theory]
        [InlineData("Account closed", "Account closed")]
        [InlineData("Причина видалення", "Причина видалення")]
        [InlineData(null, "No deletion reason was provided.")]
        public void CreateThenRestore_EquivalentReasons_PreservesEqualityAndHashCode(
            string? rawReason,
            string expectedValue)
        {
            var firstReason = CreateReason(rawReason);
            var secondReason = DeletionReason.Restore(expectedValue);

            var created = DeleteUserAction.Create(firstReason);
            var restored = DeleteUserAction.Restore(secondReason);

            Assert.NotSame(
                created,
                restored
            );

            Assert.NotSame(
                created.Reason,
                restored.Reason
            );

            Assert.Equal(
                expectedValue,
                created.Value
            );

            Assert.Equal(
                expectedValue,
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
        [InlineData("Account closed", "Other reason")]
        [InlineData("Account closed", "account closed")]
        [InlineData("Account closed", "Account  closed")]
        public void Equals_DifferentReasonValues_ReturnsFalse(
            string firstRawReason,
            string secondRawReason)
        {
            var first = DeleteUserAction.Create(CreateReason(firstRawReason));
            var second = DeleteUserAction.Create(CreateReason(secondRawReason));

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
    }
}
