using Authorization.Domain.Common.Errors.Common;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken
{
    public class UserRefreshToken : Entity<UserRefreshTokenId>
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromDays(7);

        public UserId UserId { get; private set; }

        public RefreshToken RefreshToken { get; private set; }

        public DeviceInfo DeviceInfo { get; private set; }

        public IpAddress IpAddress { get; private set; }

        public string? City { get; private set; }

        public TokenStatus TokenStatus { get; private set; }

        public DateTimeOffset ExpiresAt { get; private set; }

        public DateTimeOffset? ConsumedAt { get; private set; }

        public DateTimeOffset? RevokedAt { get; private set; }

        #region Creation
        private UserRefreshToken(
            UserRefreshTokenId userRefreshTokenId,
            UserId userId,
            RefreshToken refreshToken,
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            DateTimeOffset now,
            string? city = null)
            : base(userRefreshTokenId, now)
        {
            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            TokenStatus = TokenStatus.Active;
            ExpiresAt = now.Add(ExpirationTime);
            ConsumedAt = null;
            RevokedAt = null;
        }

        internal static Result<UserRefreshToken> Create(
            UserId userId,
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            DateTimeOffset now,
            string? city = null)
        {
            if (userId is null)
                return Result<UserRefreshToken>.Failure(CommonErrors.Identifier.EmptyIdentifier<UserRefreshToken>(nameof(userId)));

            if (deviceInfo is null)
                return Result<UserRefreshToken>.Failure(RefreshTokenErrors.Device.EmptyDeviceInfo<UserRefreshToken>());

            if (ipAddress is null)
                return Result<UserRefreshToken>.Failure(IpAddressErrors.EmptyIpAddress<UserRefreshToken>());

            var createRefreshToken = new UserRefreshToken(
                UserRefreshTokenId.Create(),
                userId,
                RefreshToken.Create(),
                deviceInfo,
                ipAddress,
                now,
                city
            );

            return Result<UserRefreshToken>.Success(createRefreshToken);
        }
        #endregion

        #region Restoration
        private UserRefreshToken(
            UserRefreshTokenId userRefreshTokenId,
            UserId userId,
            RefreshToken refreshToken,
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            string? city,
            TokenStatus tokenStatus,
            DateTimeOffset expiresAt,
            DateTimeOffset? consumedAt,
            DateTimeOffset createdAt,
            DateTimeOffset? revokedAt)
            : base(userRefreshTokenId, createdAt)
        {
            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            TokenStatus = tokenStatus;
            ExpiresAt = expiresAt;
            ConsumedAt = consumedAt;
            RevokedAt = revokedAt;
        }

        public static UserRefreshToken Restore(
            UserRefreshTokenId userRefreshTokenId,
            UserId userId,
            RefreshToken refreshToken,
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            string? city,
            TokenStatus tokenStatus,
            DateTimeOffset expiresAt,
            DateTimeOffset? consumedAt,
            DateTimeOffset createdAt,
            DateTimeOffset? revokedAt)
        {
            if (!Enum.IsDefined(typeof(TokenStatus), tokenStatus))
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<UserRefreshToken>(nameof(tokenStatus), tokenStatus.ToString());

            ValidateConsistency(createdAt, expiresAt, consumedAt, revokedAt);

            DomainGuard.AgainstNull<UserRefreshToken>(
                (userRefreshTokenId, nameof(userRefreshTokenId)),
                (userId, nameof(userId)),
                (refreshToken, nameof(refreshToken)),
                (deviceInfo, nameof(deviceInfo)),
                (ipAddress, nameof(ipAddress))
            );

            return new UserRefreshToken(
                userRefreshTokenId,
                userId,
                refreshToken,
                deviceInfo,
                ipAddress,
                city,
                tokenStatus,
                expiresAt,
                consumedAt,
                createdAt,
                revokedAt
            );
        }

        private static void ValidateConsistency(
            DateTimeOffset createdAt,
            DateTimeOffset expiresAt,
            DateTimeOffset? consumedAt,
            DateTimeOffset? revokedAt)
        {
            if (expiresAt < createdAt)
                throw DomainDataInconsistencyException.ValueOutOfRange<UserRefreshToken>(nameof(expiresAt), expiresAt);

            if (consumedAt is not null && consumedAt < createdAt)
                throw DomainDataInconsistencyException.ValueOutOfRange<UserRefreshToken>(nameof(consumedAt), consumedAt);

            if (revokedAt is not null && revokedAt < createdAt)
                throw DomainDataInconsistencyException.ValueOutOfRange<UserRefreshToken>(nameof(revokedAt), revokedAt);
        }
        #endregion

        #region Behavior
        public bool IsActive() => TokenStatus == TokenStatus.Active;

        internal Result Consume(DateTimeOffset now)
        {
            if (IsExpired(now))
            {
                TokenStatus = TokenStatus.Expired;

                return Result.Failure(RefreshTokenErrors.TokenStatus.ExpiredToken<UserRefreshToken>());
            }
                
            TokenStatus = TokenStatus.Consumed;
            ConsumedAt = now;

            return Result.Success();
        }

        internal Result Revoke(DateTimeOffset now)
        {
            TokenStatus = TokenStatus.Revoked;
            RevokedAt = now;

            return Result.Success();
        }
        #endregion

        #region Guard
        private bool IsExpired(DateTimeOffset now)
            => ExpiresAt <= now;
        #endregion
    }
}
