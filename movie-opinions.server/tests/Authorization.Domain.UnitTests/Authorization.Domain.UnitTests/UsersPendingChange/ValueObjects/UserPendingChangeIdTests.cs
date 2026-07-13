using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.UsersPendingChange.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersPendingChange.ValueObjects
{
    public class UserPendingChangeIdTests
    {
        #region Creation
        [Fact]
        public void CreateUnique_ShouldReturnVersion7Guid()
        {
            var id = UserPendingChangeId.CreateUnique();

            id.Value.Should().NotBe(Guid.Empty);
            id.Value.Version.Should().Be(7);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldThrowException_WhenGuidIsEmpty()
        {
            Action act = () => UserPendingChangeId.Restore(Guid.Empty);

            act.Should().Throw<DomainDataInconsistencyException>();
        }

        [Fact]
        public void Restore_ShouldCreateInstance_WhenGuidIsValid()
        {
            var guid = Guid.CreateVersion7();
            var id = UserPendingChangeId.Restore(guid);

            id.Value.Should().Be(guid);
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeEqual_WhenValuesAreSame()
        {
            var guid = Guid.CreateVersion7();
            var id1 = UserPendingChangeId.Restore(guid);
            var id2 = UserPendingChangeId.Restore(guid);

            id1.Should().Be(id2);
            (id1 == id2).Should().BeTrue();
            id1.GetHashCode().Should().Be(id2.GetHashCode());
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenValuesAreDifferent()
        {
            var id1 = UserPendingChangeId.Restore(Guid.CreateVersion7());
            var id2 = UserPendingChangeId.Restore(Guid.CreateVersion7());

            (id1 != id2).Should().BeTrue();
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnGuidValue()
        {
            var guid = Guid.CreateVersion7();
            var id = UserPendingChangeId.Restore(guid);

            var components = id.GetEqualityComponents();

            components.Should().ContainSingle().Which.Should().Be(guid);
        }
        #endregion

        #region Operators
        [Fact]
        public void ImplicitOperator_ShouldConvertToGuid()
        {
            var guid = Guid.CreateVersion7();
            var id = UserPendingChangeId.Restore(guid);

            Guid result = id;

            result.Should().Be(guid);
        }
        #endregion
    }
}
