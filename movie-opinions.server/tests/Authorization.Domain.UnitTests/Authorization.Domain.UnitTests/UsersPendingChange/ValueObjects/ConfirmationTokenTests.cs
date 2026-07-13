using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.UsersPendingChange.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersPendingChange.ValueObjects
{
    public class ConfirmationTokenTests
    {
        #region Creation
        [Fact]
        public void CreateUnique_ShouldGenerateValidToken()
        {
            var token = ConfirmationToken.CreateUnique();

            token.Should().NotBeNull();
            token.Value.Should().NotBeNullOrWhiteSpace();
            token.Value.Length.Should().BeGreaterThan(80);
        }

        [Fact]
        public void CreateUnique_ShouldGenerateUniqueTokens()
        {
            var token1 = ConfirmationToken.CreateUnique();
            var token2 = ConfirmationToken.CreateUnique();

            token1.Value.Should().NotBe(token2.Value);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateToken_WhenValueIsValid()
        {
            var original = ConfirmationToken.CreateUnique();
            var restored = ConfirmationToken.Restore(original.Value);

            restored.Value.Should().Be(original.Value);
            restored.Should().BeEquivalentTo(original);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Restore_ShouldThrowException_WhenValueIsEmpty(string? invalidValue)
        {
            Action act = () => ConfirmationToken.Restore(invalidValue!);

            act.Should().Throw<DomainDataInconsistencyException>();
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeEqual_WhenValuesAreSame()
        {
            var token1 = ConfirmationToken.Restore("same-token-value-123");
            var token2 = ConfirmationToken.Restore("same-token-value-123");
            var token3 = ConfirmationToken.Restore("different-token-value");

            token1.Should().Be(token2);
            token1.GetHashCode().Should().Be(token2.GetHashCode());

            token1.Should().NotBe(token3);
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnValue()
        {
            var token = ConfirmationToken.Restore("test-confirmation-token");

            var components = token.GetEqualityComponents();

            components.Should().ContainSingle().Which.Should().Be("test-confirmation-token");
        }
        #endregion

        #region Security
        [Fact]
        public void CreateUnique_ShouldUseCryptographicallySecureRandomness()
        {
            var tokens = new HashSet<string>();

            for (int i = 0; i < 50; i++)
            {
                var token = ConfirmationToken.CreateUnique();
                tokens.Add(token.Value).Should().BeTrue();
            }

            tokens.Count.Should().Be(50);
        }
        #endregion
    }
}
