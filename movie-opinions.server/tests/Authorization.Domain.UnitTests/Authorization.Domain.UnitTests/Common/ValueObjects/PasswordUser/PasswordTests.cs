using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.PasswordUser;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.PasswordUser
{
    public class PasswordTests
    {
        [Fact]
        public void Create_NullHash_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Password.Create(null!)
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
                "Password",
                exception.Context["Type"]
            );

            Assert.Equal(
                "hash",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("TEST.HASH")]
        [InlineData("AbCdEf0123+/=")]
        [InlineData(" TEST.HASH ")]
        public void Create_ValidHash_SetsValue(string hashValue)
        {
            var hash = PasswordHash.Create(hashValue);

            var password = Password.Create(hash);

            Assert.Equal(
                hashValue,
                password.Value
            );
        }

        [Fact]
        public void Restore_NullHash_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Password.Restore(null!)
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
                "Password",
                exception.Context["Type"]
            );

            Assert.Equal(
                "hash",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("TEST.HASH")]
        [InlineData("AbCdEf0123+/=")]
        [InlineData(" TEST.HASH ")]
        public void Restore_ValidHash_SetsValue(string hashValue)
        {
            var hash = PasswordHash.Restore(hashValue);

            var password = Password.Restore(hash);

            Assert.Equal(
                hashValue,
                password.Value
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Matches_VerifierResult_ReturnsSameResult(bool expectedResult)
        {
            var hash = PasswordHash.Create("TEST.HASH");
            var password = Password.Create(hash);
            var plainResult = PlainPassword.Create("Abcdefg1");

            Assert.True(plainResult.IsSuccess);

            var callCount = 0;
            var matches = password.Matches(
                plainResult.Value,
                (_, _) =>
                {
                    callCount++;
                    return expectedResult;
                }
            );

            Assert.Equal(
                expectedResult,
                matches
            );

            Assert.Equal(
                1,
                callCount
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Matches_ValidArguments_PassesOriginalInstancesToVerifier(
            bool useCreate)
        {
            var hash = PasswordHash.Create("TEST.HASH");
            var password = useCreate
                ? Password.Create(hash)
                : Password.Restore(hash);

            var plainResult = PlainPassword.Create("Abcdefg1");

            Assert.True(plainResult.IsSuccess);

            var plainPassword = plainResult.Value;

            PlainPassword? receivedPlain = null;
            PasswordHash? receivedHash = null;

            password.Matches(
                plainPassword,
                (plain, storedHash) =>
                {
                    receivedPlain = plain;
                    receivedHash = storedHash;
                    return true;
                }
            );

            Assert.Same(
                plainPassword,
                receivedPlain
            );

            Assert.Same(
                hash,
                receivedHash
            );
        }

        [Fact]
        public void Matches_NullPlainPassword_ThrowsDomainDataInconsistencyException()
        {
            var hash = PasswordHash.Create("TEST.HASH");
            var password = Password.Create(hash);
            var callCount = 0;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                password.Matches(
                    null!,
                    (_, _) =>
                    {
                        callCount++;
                        return true;
                    }
                )
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
                "Password",
                exception.Context["Type"]
            );

            Assert.Equal(
                "plainPassword",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Compare",
                exception.Context["Operation"]
            );

            Assert.Equal(
                0,
                callCount
            );
        }

        [Fact]
        public void Matches_NullVerifier_ThrowsDomainDataInconsistencyException()
        {
            var hash = PasswordHash.Create("TEST.HASH");
            var password = Password.Create(hash);
            var plainResult = PlainPassword.Create("Abcdefg1");

            Assert.True(plainResult.IsSuccess);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                password.Matches(plainResult.Value, null!)
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
                "Password",
                exception.Context["Type"]
            );

            Assert.Equal(
                "verifier",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Compare",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Matches_BothArgumentsNull_ReportsPlainPassword()
        {
            var hash = PasswordHash.Create("TEST.HASH");
            var password = Password.Create(hash);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                password.Matches(null!, null!)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "Password",
                exception.Context["Type"]
            );

            Assert.Equal(
                "plainPassword",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Compare",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Matches_VerifierThrows_PropagatesSameException()
        {
            var hash = PasswordHash.Create("TEST.HASH");
            var password = Password.Create(hash);
            var plainResult = PlainPassword.Create("Abcdefg1");

            Assert.True(plainResult.IsSuccess);

            var expectedException = new InvalidOperationException(
                "Test verifier failure."
            );

            var thrown = Assert.Throws<InvalidOperationException>(() =>
                password.Matches(
                    plainResult.Value,
                    (_, _) => throw expectedException
                )
            );

            Assert.Same(
                expectedException,
                thrown
            );
        }

        [Fact]
        public void Matches_RepeatedCalls_UsesCurrentArgumentsAndVerifier()
        {
            var hash = PasswordHash.Create("TEST.HASH");
            var password = Password.Create(hash);

            var firstPlainResult = PlainPassword.Create("Abcdefg1");
            var secondPlainResult = PlainPassword.Create("Abcdefg2");

            Assert.True(firstPlainResult.IsSuccess);
            Assert.True(secondPlainResult.IsSuccess);

            var firstCallCount = 0;
            PlainPassword? firstReceivedPlain = null;
            PasswordHash? firstReceivedHash = null;

            var firstMatches = password.Matches(
                firstPlainResult.Value,
                (plain, storedHash) =>
                {
                    firstCallCount++;
                    firstReceivedPlain = plain;
                    firstReceivedHash = storedHash;
                    return false;
                }
            );

            var secondCallCount = 0;
            PlainPassword? secondReceivedPlain = null;
            PasswordHash? secondReceivedHash = null;

            var secondMatches = password.Matches(
                secondPlainResult.Value,
                (plain, storedHash) =>
                {
                    secondCallCount++;
                    secondReceivedPlain = plain;
                    secondReceivedHash = storedHash;
                    return true;
                }
            );

            Assert.False(firstMatches);
            Assert.True(secondMatches);

            Assert.Equal(
                1,
                firstCallCount
            );

            Assert.Equal(
                1,
                secondCallCount
            );

            Assert.Same(
                firstPlainResult.Value,
                firstReceivedPlain
            );

            Assert.Same(
                secondPlainResult.Value,
                secondReceivedPlain
            );

            Assert.Same(
                hash,
                firstReceivedHash
            );

            Assert.Same(
                hash,
                secondReceivedHash
            );
        }

        [Fact]
        public void GetEqualityComponents_ValidPassword_ReturnsSingleHashString()
        {
            var password = Password.Create(
                PasswordHash.Create("TEST.HASH")
            );

            var component = Assert.Single(password.GetEqualityComponents());

            Assert.Equal(
                "TEST.HASH",
                component
            );

            Assert.IsType<string>(component);
        }

        [Theory]
        [InlineData("TEST.HASH.1", "TEST.HASH.2")]
        [InlineData("TEST.HASH", "test.hash")]
        [InlineData("TEST.HASH", " TEST.HASH ")]
        public void Equals_DifferentHashValues_ReturnsFalse(
            string firstHashValue,
            string secondHashValue)
        {
            var first = Password.Create(
                PasswordHash.Create(firstHashValue)
            );
            var second = Password.Create(
                PasswordHash.Create(secondHashValue)
            );

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }

        [Fact]
        public void CreateThenRestore_EquivalentHashes_PreservesEqualityAndHashCode()
        {
            var firstHash = PasswordHash.Create("TEST.HASH");
            var secondHash = PasswordHash.Create("TEST.HASH");

            var created = Password.Create(firstHash);
            var restored = Password.Restore(secondHash);

            Assert.Equal(
                "TEST.HASH",
                created.Value
            );

            Assert.Equal(
                "TEST.HASH",
                restored.Value
            );

            Assert.NotSame(
                firstHash,
                secondHash
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
    }
}
