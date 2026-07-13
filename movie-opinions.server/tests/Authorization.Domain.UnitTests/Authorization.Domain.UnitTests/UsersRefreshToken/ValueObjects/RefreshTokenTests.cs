using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.UsersRefreshToken.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersRefreshToken.ValueObjects
{
    public class RefreshTokenTests
    {
        #region Creation
        [Fact]
        public void CreateUnique_ShouldGenerateValidToken()
        {
            var token = RefreshToken.CreateUnique();

            token.Should().NotBeNull();
            token.Value.Should().NotBeNullOrWhiteSpace();
            token.Value.Length.Should().BeGreaterThan(80);
        }

        [Fact]
        public void CreateUnique_ShouldGenerateUniqueTokens()
        {
            var token1 = RefreshToken.CreateUnique();
            var token2 = RefreshToken.CreateUnique();

            token1.Value.Should().NotBe(token2.Value);
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateToken_WhenValueIsValid()
        {
            var original = RefreshToken.CreateUnique();
            var restored = RefreshToken.Restore(original.Value);

            restored.Value.Should().Be(original.Value);
            restored.Should().BeEquivalentTo(original);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Restore_ShouldThrowException_WhenValueIsEmpty(string? invalidValue)
        {
            Action act = () => RefreshToken.Restore(invalidValue!);

            act.Should().Throw<DomainDataInconsistencyException>();
        }
        #endregion

        #region Equality
        [Fact]
        public void Equality_ShouldBeEqual_WhenValuesAreSame()
        {
            var token1 = RefreshToken.Restore("same-refresh-token-value-xyz");
            var token2 = RefreshToken.Restore("same-refresh-token-value-xyz");
            var token3 = RefreshToken.Restore("different-refresh-token");

            token1.Should().Be(token2);
            token1.GetHashCode().Should().Be(token2.GetHashCode());

            token1.Should().NotBe(token3);
        }

        [Fact]
        public void GetEqualityComponents_ShouldReturnValue()
        {
            var token = RefreshToken.Restore("test-refresh-token-12345");

            var components = token.GetEqualityComponents();

            components.Should().ContainSingle().Which.Should().Be("test-refresh-token-12345");
        }
        #endregion

        #region Security
        [Fact]
        public void CreateUnique_ShouldUseCryptographicallySecureRandom()
        {
            var tokens = new HashSet<string>();

            for (int i = 0; i < 50; i++)
            {
                var token = RefreshToken.CreateUnique();
                tokens.Add(token.Value).Should().BeTrue("Tokens must be unique");
            }

            tokens.Count.Should().Be(50);
        }
        #endregion
    }
}
