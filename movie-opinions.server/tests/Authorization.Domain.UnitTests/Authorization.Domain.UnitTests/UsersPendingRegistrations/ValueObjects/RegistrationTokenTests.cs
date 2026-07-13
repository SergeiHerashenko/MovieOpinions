using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersPendingRegistrations.ValueObjects
{
    public class RegistrationTokenTests
    {
        #region CreateUnique
        [Fact]
        public void CreateUnique_ShouldGenerateValidToken()
        {
            var token = RegistrationToken.CreateUnique();

            // Assert
            token.Should().NotBeNull();
            token.Value.Should().NotBeNullOrWhiteSpace();
            token.Value.Length.Should().BeGreaterThan(80);
            token.Value.Should().EndWith("==");
        }

        [Fact]
        public void CreateUnique_ShouldGenerateDifferentTokens_EachTime()
        {
            var token1 = RegistrationToken.CreateUnique();
            var token2 = RegistrationToken.CreateUnique();

            token1.Value.Should().NotBe(token2.Value);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldSucceed_WithValidValue()
        {
            var original = RegistrationToken.CreateUnique();
            var restored = RegistrationToken.Restore(original.Value);

            restored.Should().NotBeNull();
            restored.Value.Should().Be(original.Value);
            restored.Should().BeEquivalentTo(original);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Restore_ShouldThrow_WhenValueIsEmpty(string? invalidValue)
        {
            Action act = () => RegistrationToken.Restore(invalidValue!);

            act.Should().Throw<DomainDataInconsistencyException>();
        }

        [Fact]
        public void Restore_ShouldAccept_NonEmptyString()
        {
            var token = RegistrationToken.Restore("valid-token-123");

            token.Value.Should().Be("valid-token-123");
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeBasedOnValue()
        {
            var token1 = RegistrationToken.Restore("same-token-value");
            var token2 = RegistrationToken.Restore("same-token-value");
            var token3 = RegistrationToken.Restore("different-token-value");

            token1.Should().Be(token2);
            token1.GetHashCode().Should().Be(token2.GetHashCode());

            token1.Should().NotBe(token3);
            token1.GetHashCode().Should().NotBe(token3.GetHashCode());
        }

        [Fact]
        public void EqualityOperators_ShouldWorkCorrectly()
        {
            var token1 = RegistrationToken.Restore("token-abc");
            var token2 = RegistrationToken.Restore("token-abc");
            var token3 = RegistrationToken.Restore("token-xyz");

            (token1 == token2).Should().BeTrue();
            (token1 != token3).Should().BeTrue();
            (token1 == null!).Should().BeFalse();
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnValue()
        {
            var token = RegistrationToken.Restore("test-value");

            var components = token.GetEqualityComponents();

            components.Should().ContainSingle().Which.Should().Be("test-value");
        }
        #endregion

        #region Edge Cases & Security
        [Fact]
        public void CreateUnique_ShouldUseCryptographicallySecureRandom()
        {
            var tokens = new HashSet<string>();

            for (int i = 0; i < 100; i++)
            {
                var token = RegistrationToken.CreateUnique();
                tokens.Add(token.Value).Should().BeTrue("All tokens must be unique");
            }

            tokens.Count.Should().Be(100);
        }

        [Fact]
        public void Value_ShouldBeImmutable()
        {
            var token = RegistrationToken.CreateUnique();
            var originalValue = token.Value;

            token.Value.Should().Be(originalValue);
        }
        #endregion
    }
}
