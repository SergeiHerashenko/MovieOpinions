using Authorization.Domain.Common.Errors.TokenError;
using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;
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

        public static DomainResult<UserRefreshToken> Create(UserId userId, DeviceInfo deviceInfo, IpAddress ipAddress, string? city)
        {
            if (userId is null)
                return DomainResult<UserRefreshToken>.Failure(TokenError.Empty(nameof(userId)));

            if (deviceInfo is null)
                return DomainResult<UserRefreshToken>.Failure(TokenError.Empty(nameof(deviceInfo)));

            if (ipAddress is null)
                return DomainResult<UserRefreshToken>.Failure(TokenError.Empty(nameof(ipAddress)));

            return DomainResult<UserRefreshToken>.Success(
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
            EnsureNotNull<UserRefreshToken>(userRefreshTokenId, nameof(userRefreshTokenId));
            EnsureNotNull<UserRefreshToken>(userId, nameof(userId));
            EnsureNotNull<UserRefreshToken>(refreshToken, nameof(refreshToken));
            EnsureNotNull<UserRefreshToken>(deviceInfo, nameof(deviceInfo));
            EnsureNotNull<UserRefreshToken>(ipAddress, nameof(ipAddress));

            return new UserRefreshToken(userRefreshTokenId, userId, refreshToken, deviceInfo, ipAddress, city, tokenStatus, expiresAt, consumedAt, createdAt, revokedAt);
        }
        #endregion

        #region Behavior
        public DomainResult Consume(DateTimeOffset now)
        {
            if (Status != TokenStatus.Active)
                return GetErrorForInvalidStatus(Status);

            Status = TokenStatus.Consumed;
            ConsumedAt = now;

            return DomainResult.Success();
        }

        public DomainResult Revoke(DateTimeOffset now)
        {
            if (Status != TokenStatus.Active)
                return GetErrorForInvalidStatus(Status);

            Status = TokenStatus.Revoked;
            RevokedAt = now;

            return DomainResult.Success();
        }

        public DomainResult Expire(DateTimeOffset now)
        {
            if (Status != TokenStatus.Active)
                return GetErrorForInvalidStatus(Status);

            if(now >= ExpiresAt)
            {
                Status = TokenStatus.Expired;

                return DomainResult.Success();
            }

            return DomainResult.Failure(TokenError.TokenIsActive("Token not yet expired"));
        }

        public bool IsActive() => Status == TokenStatus.Active;
        #endregion

        #region Guards
        private static void EnsureNotNull<T>(object? value, string name)
        {
            if (value is null)
                throw DomainDataInconsistencyException.EmptyOnRestore<T>(
                    name
                );
        }

        private static DomainResult GetErrorForInvalidStatus(TokenStatus tokenStatus) => tokenStatus switch
        {
            TokenStatus.Consumed => DomainResult.Failure(TokenError.UsedToken("Token already used!")),
            TokenStatus.Expired => DomainResult.Failure(TokenError.ExpiredToken("The token's lifetime has expired!")),
            TokenStatus.Revoked => DomainResult.Failure(TokenError.RevokedToken("The token was manually revoked by the user or system")),
            _ => DomainResult.Failure(TokenError.InvalidFormat($"Unsupported token status: {tokenStatus}"))
        };
        #endregion
    }
}
