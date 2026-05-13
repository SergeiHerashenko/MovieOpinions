using Authorization.Domain.Common;
using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;
using Authorization.Domain.ValueObjects;
using Authorization.Domain.ValueObjects.Login;

namespace Authorization.Domain.Entities
{
    public class UserRefreshToken : BaseEntity
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromDays(7);

        public Guid UserId { get; private set; }

        public string RefreshToken { get; private set; }

        public DeviceInfo DeviceInfo { get; private set; }

        public string IpAddress { get; private set; }

        public string? City { get; private set; }

        public DateTimeOffset ExpiresAt { get; private set; }

        public bool IsUsed { get; private set; }

        public bool IsRevoked { get; private set; }

        #region Creation
        private UserRefreshToken(Guid userId, string refreshToken, DeviceInfo deviceInfo, string ipAddress, string? city)
            : base()
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ValidationDomainException(
                    DomainErrorCodes.TokenError.TokenEmpty,
                    $"{nameof(refreshToken)} validation failed: value is null. Entity {nameof(UserRefreshToken)}!"
                );

            if (userId == Guid.Empty)
                throw new ValidationDomainException(
                    DomainErrorCodes.IdentifierError.Empty,
                    $"{nameof(userId)} validation failed: value is null. Entity {nameof(UserRefreshToken)}!"
                );

            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            ExpiresAt = CreatedAt.Add(ExpirationTime);
            IsUsed = false;
            IsRevoked = false;
        }

        public static UserRefreshToken CreateRefreshToken(Guid userId, string refreshToken, DeviceInfo deviceInfo, string ipAddress, string? city)
        {
            return new UserRefreshToken(userId, refreshToken, deviceInfo, ipAddress, city);
        }
        #endregion

        #region Restore
        private UserRefreshToken(
            Guid id,
            Guid userId,
            string refreshToken,
            DeviceInfo deviceInfo,
            string ipAddress,
            string? city,
            DateTimeOffset expiresAt,
            DateTimeOffset createdAt,
            bool isUsed,
            bool isRevoked)
            : base(id, createdAt)
        {
            if (userId == Guid.Empty)
                throw new DataInconsistencyDomainException(
                    DomainErrorCodes.RestoreError.NullReference,
                    $"Missing required field {nameof(userId)} during {nameof(UserRefreshToken)} entity reconstruction!"
                );

            if (refreshToken is null)
                throw new DataInconsistencyDomainException(
                    DomainErrorCodes.RestoreError.NullReference,
                    $"Missing required field {nameof(refreshToken)} during {nameof(UserDeletion)} entity reconstruction!"
                );

            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            ExpiresAt = expiresAt;
            IsUsed = isUsed;
            IsRevoked = isRevoked;
        }

        public static UserRefreshToken Restore(Guid id,
            Guid userId,
            string refreshToken,
            DeviceInfo deviceInfo,
            string ipAddress,
            string? city,
            DateTimeOffset expiresAt,
            DateTimeOffset createdAt,
            bool isUsed,
            bool isRevoked)
        {
            return new UserRefreshToken(id, userId, refreshToken, deviceInfo, ipAddress, city, expiresAt, createdAt, isUsed, isRevoked);
        }
        #endregion

        #region Behavior
        public void Revoke()
        {
            if (IsRevoked) return;
            IsRevoked = true;
        }

        public void Use(DateTimeOffset now)
        {
            if (!IsActive(now))
                throw new BusinessRuleViolationDomainException(
                    DomainErrorCodes.TokenError.TokenInvalid,
                    "Refresh token is no longer active!"
                );

            IsUsed = true;
        }

        public bool IsActive(DateTimeOffset now) => !IsUsed && !IsRevoked && now < ExpiresAt;
        #endregion
    }
}