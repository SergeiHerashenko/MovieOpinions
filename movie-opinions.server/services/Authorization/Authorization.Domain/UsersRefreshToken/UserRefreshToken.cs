using Authorization.Domain.Common.Errors.TokenError;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validations;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersRefreshToken.Enums;
using Authorization.Domain.UsersRefreshToken.ValueObjects;

namespace Authorization.Domain.UsersRefreshToken
{
    public class UserRefreshToken : AggregateRoot<UserRefreshTokenId, Guid>
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromDays(7);

        public UserId UserId { get; private set; }

        public RefreshToken RefreshToken { get; private set; }

        public DeviceInfo DeviceInfo { get; private set; }

        public IpAddress IpAddress { get; private set; }

        public string? City { get; private set; }

        public TokenStatus Status { get; private set; }

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
            string? city)
            : base(userRefreshTokenId)
        {
            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            Status = TokenStatus.Active;
            ExpiresAt = CreatedAt.Add(ExpirationTime);
            ConsumedAt = null;
            RevokedAt = null;
        }

        public static Result<UserRefreshToken> Create(UserId userId, DeviceInfo deviceInfo, IpAddress ipAddress, string? city)
        {
            if (userId is null)
                return Result<UserRefreshToken>.Failure(TokenError.Empty<UserRefreshToken>(nameof(userId)));

            if (deviceInfo is null)
                return Result<UserRefreshToken>.Failure(TokenError.Empty<UserRefreshToken>(nameof(deviceInfo)));

            if (ipAddress is null)
                return Result<UserRefreshToken>.Failure(TokenError.Empty<UserRefreshToken>(nameof(ipAddress)));

            return Result<UserRefreshToken>.Success(
                new UserRefreshToken(
                    UserRefreshTokenId.CreateUnique(), 
                    userId, 
                    RefreshToken.CreateUnique(), 
                    deviceInfo, 
                    ipAddress, 
                    city
                )
            );
        }
        #endregion

        #region Restore
        private UserRefreshToken(
            UserRefreshTokenId userRefreshTokenId,
            UserId userId,
            RefreshToken refreshToken,
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            string? city,
            TokenStatus tokenStatus,
            DateTimeOffset expiresAt,
            DateTimeOffset? сonsumedAt,
            DateTimeOffset createdAt,
            DateTimeOffset? revokedAt)
            : base(userRefreshTokenId, createdAt)
        {
            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            Status = tokenStatus;
            ExpiresAt = expiresAt;
            ConsumedAt = сonsumedAt;
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
            DomainGuard.AgainstNull<UserRefreshToken>(
                (userRefreshTokenId, nameof(userRefreshTokenId)),
                (userId, nameof(userId)),
                (refreshToken, nameof(refreshToken)),
                (deviceInfo, nameof(deviceInfo)),
                (ipAddress, nameof(ipAddress))
            );

            return new UserRefreshToken(userRefreshTokenId, userId, refreshToken, deviceInfo, ipAddress, city, tokenStatus, expiresAt, consumedAt, createdAt, revokedAt);
        }
        #endregion

        #region Behavior
        public Result Consume(DateTimeOffset now)
        {
            if (Status != TokenStatus.Active)
                return GetErrorForInvalidStatus(Status);

            Status = TokenStatus.Consumed;
            ConsumedAt = now;

            return Result.Success();
        }

        public Result Revoke(DateTimeOffset now)
        {
            if (Status != TokenStatus.Active)
                return GetErrorForInvalidStatus(Status);

            Status = TokenStatus.Revoked;
            RevokedAt = now;

            return Result.Success();
        }

        public Result Expire(DateTimeOffset now)
        {
            if (Status != TokenStatus.Active)
                return GetErrorForInvalidStatus(Status);

            if(now >= ExpiresAt)
            {
                Status = TokenStatus.Expired;

                return Result.Success();
            }

            return Result.Failure(TokenError.TokenIsActive<UserRefreshToken>());
        }

        public bool IsActive() => Status == TokenStatus.Active;
        #endregion

        #region Guards
        private static Result GetErrorForInvalidStatus(TokenStatus tokenStatus) => tokenStatus switch
        {
            TokenStatus.Consumed => Result.Failure(TokenError.UsedToken<UserRefreshToken>()),
            TokenStatus.Expired => Result.Failure(TokenError.ExpiredToken<UserRefreshToken>()),
            TokenStatus.Revoked => Result.Failure(TokenError.RevokedToken<UserRefreshToken>()),
            _ => Result.Failure(TokenError.InvalidFormat<UserRefreshToken>(tokenStatus.ToString()))
        };
        #endregion
    }
}
