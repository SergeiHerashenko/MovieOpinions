using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users.ValueObjects
{
    public class UserIdTests
    {
        #region Creation
        [Fact]
        public void CreateUnique_ShouldReturnValidGuid()
        {
            var userId = UserId.CreateUnique();

            userId.Value.Should().NotBe(Guid.Empty);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldThrowException_WhenGuidIsEmpty()
        {
            Action act = () => UserId.Restore(Guid.Empty);

            act.Should().Throw<DomainDataInconsistencyException>();
        }

        [Fact]
        public void Restore_ShouldReturnInstance_WhenGuidIsValid()
        {
            var guid = Guid.CreateVersion7();
            var userId = UserId.Restore(guid);

            userId.Value.Should().Be(guid);
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeTrue_WhenGuidsAreSame()
        {
            var guid = Guid.CreateVersion7();
            var id1 = UserId.Restore(guid);
            var id2 = UserId.Restore(guid);

            id1.Should().Be(id2);
            (id1 == id2).Should().BeTrue();
        }
        #endregion

        #region Operators
        [Fact]
        public void ImplicitOperator_ShouldReturnGuidValue()
        {
            var guid = Guid.CreateVersion7();
            var userId = UserId.Restore(guid);

            Guid result = userId;

            result.Should().Be(guid);
        }
        #endregion
    }
}
