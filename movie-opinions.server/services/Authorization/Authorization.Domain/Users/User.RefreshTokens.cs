using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Tokens;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        public Result<UserRefreshToken> CreateRefreshToken(
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            DateTimeOffset now,
            string? city = null)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return Result<UserRefreshToken>.Failure(access.Errors);

            if (IsNewDevice(deviceInfo, ipAddress))
                AddDomainEvent(new UserSignedInFromNewDeviceEvent(Id, Login, deviceInfo, ipAddress, now));

            var tokenResult = UserRefreshToken.Create(Id, deviceInfo, ipAddress, now, city);

            if (tokenResult.IsFailure)
                return tokenResult;

            var refreshToken = tokenResult.Value;

            _refreshTokens.Add(refreshToken);

            return Result<UserRefreshToken>.Success(refreshToken);
        }

        public Result ConsumeRefreshToken(UserRefreshTokenId userRefreshTokenId, DateTimeOffset now)
        {
            var refreshToken = _refreshTokens
                .FirstOrDefault(x => x.Id == userRefreshTokenId);

            if (refreshToken is null)
                return Result.Failure(RefreshTokenErrors.TokenStatus.NotFoundToken<User>());

            var consumeResult = refreshToken.Consume(now);

            if (consumeResult.IsFailure)
                return consumeResult;

            _refreshTokens.Remove(refreshToken);

            AddAggregateChange(new UserRefreshTokenUpdated(
                refreshToken.Id,
                Id,
                refreshToken.TokenStatus,
                refreshToken.ConsumedAt,
                refreshToken.RevokedAt,
                now)
            );

            return Result.Success();
        }

        public Result RevokeRefreshToken(UserRefreshTokenId userRefreshTokenId, DateTimeOffset now)
        {
            var refreshToken = _refreshTokens
                .FirstOrDefault(x => x.Id == userRefreshTokenId);

            if (refreshToken is null)
                return Result.Failure(RefreshTokenErrors.TokenStatus.NotFoundToken<User>());

            var consumeResult = refreshToken.Revoke(now);

            if (consumeResult.IsFailure)
                return consumeResult;

            _refreshTokens.Remove(refreshToken);

            AddAggregateChange(new UserRefreshTokenUpdated(
                refreshToken.Id,
                Id,
                refreshToken.TokenStatus,
                refreshToken.ConsumedAt,
                refreshToken.RevokedAt,
                now)
            );

            return Result.Success();
        }

        private bool IsNewDevice(DeviceInfo deviceInfo, IpAddress ipAddress)
        {
            return !_refreshTokens.Any(x =>
                x.DeviceInfo == deviceInfo &&
                x.IpAddress == ipAddress
            );
        }
    }
}
