using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersRefreshToken;
using Authorization.Domain.UsersRefreshToken.Enums;
using Authorization.Domain.UsersRefreshToken.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersRefreshToken
{
    public class UserRefreshTokenTests
    {
        private readonly UserId _validUserId = UserId.CreateUnique();
        private readonly DeviceInfo _deviceInfo = DeviceInfo.Create("Desktop", "Windows 11", "Chrome", "PC").Value;
        private readonly IpAddress _ipAddress = IpAddress.Create("192.168.1.1").Value;
        private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

        #region Creation
        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            var result = UserRefreshToken.Create(_validUserId, _deviceInfo, _ipAddress, "Kyiv");

            result.IsSuccess.Should().BeTrue();
            var token = result.Value;

            token.UserId.Should().Be(_validUserId);
            token.DeviceInfo.Should().Be(_deviceInfo);
            token.IpAddress.Should().Be(_ipAddress);
            token.City.Should().Be("Kyiv");
            token.Status.Should().Be(TokenStatus.Active);
            token.RefreshToken.Should().NotBeNull();
            token.ExpiresAt.Should().BeCloseTo(_now.AddDays(7), TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenUserIdIsNull()
        {
            var result = UserRefreshToken.Create(null!, _deviceInfo, _ipAddress, null);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "TOKEN_EMPTY");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenDeviceInfoIsNull()
        {
            var result = UserRefreshToken.Create(_validUserId, null!, _ipAddress, null);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "TOKEN_EMPTY");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenIpAddressIsNull()
        {
            var result = UserRefreshToken.Create(_validUserId, _deviceInfo, null!, null);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "TOKEN_EMPTY");
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateValidInstance()
        {
            var id = UserRefreshTokenId.CreateUnique();
            var refreshToken = RefreshToken.CreateUnique();
            var createdAt = _now.AddDays(-3);

            var token = UserRefreshToken.Restore(
                id, _validUserId, refreshToken, _deviceInfo, _ipAddress, "Lviv",
                TokenStatus.Active, createdAt.AddDays(7), null, createdAt, null);

            token.Id.Should().Be(id);
            token.Status.Should().Be(TokenStatus.Active);
            token.ExpiresAt.Should().BeCloseTo(createdAt.AddDays(7), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Restore_ShouldThrowException_WhenRequiredFieldIsNull()
        {
            var id = UserRefreshTokenId.CreateUnique();
            var refreshToken = RefreshToken.CreateUnique();

            Action act = () => UserRefreshToken.Restore(
                id, null!, refreshToken, _deviceInfo, _ipAddress, null,
                TokenStatus.Active, _now.AddDays(7), null, _now, null);

            act.Should().Throw<DomainDataInconsistencyException>();
        }
        #endregion

        #region Behavior
        [Fact]
        public void Consume_ShouldReturnSuccess_WhenTokenIsActive()
        {
            var token = CreateValidRefreshToken();
            var consumeTime = _now.AddHours(2);

            var result = token.Consume(consumeTime);

            result.IsSuccess.Should().BeTrue();
            token.Status.Should().Be(TokenStatus.Consumed);
            token.ConsumedAt.Should().Be(consumeTime);
        }

        [Fact]
        public void Revoke_ShouldReturnSuccess_WhenTokenIsActive()
        {
            var token = CreateValidRefreshToken();
            var revokeTime = _now.AddHours(5);

            var result = token.Revoke(revokeTime);

            result.IsSuccess.Should().BeTrue();
            token.Status.Should().Be(TokenStatus.Revoked);
            token.RevokedAt.Should().Be(revokeTime);
        }

        [Fact]
        public void Expire_ShouldReturnSuccess_WhenTimeIsExceeded()
        {
            var token = CreateValidRefreshToken();
            var expireTime = token.ExpiresAt.AddMinutes(1);

            var result = token.Expire(expireTime);

            result.IsSuccess.Should().BeTrue();
            token.Status.Should().Be(TokenStatus.Expired);
        }

        [Fact]
        public void IsActive_ShouldReturnTrue_OnlyForActiveStatus()
        {
            var token = CreateValidRefreshToken();
            token.IsActive().Should().BeTrue();

            token.Consume(_now);
            token.IsActive().Should().BeFalse();
        }
        #endregion

        #region Invalid State Transitions
        [Fact]
        public void Consume_ShouldReturnFailure_WhenTokenAlreadyConsumed()
        {
            var token = CreateValidRefreshToken();
            token.Consume(_now);

            var result = token.Consume(_now.AddMinutes(1));

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "TOKEN_CONSUMED");
        }

        [Fact]
        public void Revoke_ShouldReturnFailure_WhenTokenAlreadyRevoked()
        {
            var token = CreateValidRefreshToken();
            token.Revoke(_now);

            var result = token.Revoke(_now.AddMinutes(1));

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "TOKEN_REVOKED");
        }
        #endregion

        #region Helpers
        private UserRefreshToken CreateValidRefreshToken()
        {
            var result = UserRefreshToken.Create(_validUserId, _deviceInfo, _ipAddress, "Kyiv");
            return result.Value;
        }
        #endregion
    }
}
