using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersPendingRegistrations.ValueObjects
{
    public class UserPendingRegistrationIdTests
    {
        #region Creation
        [Fact]
        public void CreateUnique_ShouldReturnValidGuid()
        {
            var id = UserPendingRegistrationId.CreateUnique();

            id.Value.Should().NotBe(Guid.Empty);
            id.Value.Version.Should().Be(7);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldThrowException_WhenGuidIsEmpty()
        {
            Action act = () => UserPendingRegistrationId.Restore(Guid.Empty);

            act.Should().Throw<DomainDataInconsistencyException>();
        }

        [Fact]
        public void Restore_ShouldReturnInstance_WhenGuidIsValid()
        {
            var guid = Guid.CreateVersion7();
            var id = UserPendingRegistrationId.Restore(guid);

            id.Value.Should().Be(guid);
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeTrue_WhenGuidsAreSame()
        {
            var guid = Guid.CreateVersion7();

            var id1 = UserPendingRegistrationId.Restore(guid);
            var id2 = UserPendingRegistrationId.Restore(guid);
            var id3 = UserPendingRegistrationId.Restore(Guid.CreateVersion7());

            id1.Should().Be(id2);
            (id1 == id2).Should().BeTrue();
            (id1 != id3).Should().BeTrue();
            id1.GetHashCode().Should().Be(id2.GetHashCode());
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnValue()
        {
            var guid = Guid.CreateVersion7();
            var id = UserPendingRegistrationId.Restore(guid);

            var components = id.GetEqualityComponents();

            components.Should().ContainSingle().Which.Should().Be(guid);
        }
        #endregion

        #region Operators
        [Fact]
        public void ImplicitOperator_ShouldReturnGuidValue()
        {
            var guid = Guid.CreateVersion7();
            var id = UserPendingRegistrationId.Restore(guid);

            Guid result = id;

            result.Should().Be(guid);
        }
        #endregion
    }
}
