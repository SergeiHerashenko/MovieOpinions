using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.UsersRestriction.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersRestriction.ValueObjects
{
    public class UserRestrictionSessionIdTests
    {
        #region Creation
        [Fact]
        public void CreateUnique_ShouldReturnVersion7Guid()
        {
            var id = UserRestrictionSessionId.CreateUnique();

            id.Value.Should().NotBe(Guid.Empty);
            id.Value.Version.Should().Be(7);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldThrowException_WhenGuidIsEmpty()
        {
            Action act = () => UserRestrictionSessionId.Restore(Guid.Empty);

            act.Should().Throw<DomainDataInconsistencyException>();
        }

        [Fact]
        public void Restore_ShouldCreateInstance_WhenGuidIsValid()
        {
            var guid = Guid.CreateVersion7();
            var id = UserRestrictionSessionId.Restore(guid);

            id.Value.Should().Be(guid);
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeEqual_WhenValuesAreSame()
        {
            var guid = Guid.CreateVersion7();
            var id1 = UserRestrictionSessionId.Restore(guid);
            var id2 = UserRestrictionSessionId.Restore(guid);

            id1.Should().Be(id2);
            (id1 == id2).Should().BeTrue();
            id1.GetHashCode().Should().Be(id2.GetHashCode());
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenValuesAreDifferent()
        {
            var id1 = UserRestrictionSessionId.Restore(Guid.CreateVersion7());
            var id2 = UserRestrictionSessionId.Restore(Guid.CreateVersion7());

            (id1 != id2).Should().BeTrue();
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnGuidValue()
        {
            var guid = Guid.CreateVersion7();
            var id = UserRestrictionSessionId.Restore(guid);

            var components = id.GetEqualityComponents();

            components.Should().ContainSingle().Which.Should().Be(guid);
        }
        #endregion

        #region Operators
        [Fact]
        public void ImplicitOperator_ShouldConvertToGuid()
        {
            var guid = Guid.CreateVersion7();
            var id = UserRestrictionSessionId.Restore(guid);

            Guid result = id;

            result.Should().Be(guid);
        }
        #endregion
    }
}
