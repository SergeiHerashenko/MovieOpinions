using Authorization.Domain.Common;
using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;
using Authorization.Domain.ValueObjects;

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

        public DateTime ExpiresAt { get; private set; }

        public bool IsUsed { get; private set; }

        public bool IsRevoked { get; private set; }

        private UserRefreshToken(Guid userId, string refreshToken, DeviceInfo deviceInfo, string ipAddress, string? city)
            : base()
        {
            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            ExpiresAt = CreatedAt.Add(ExpirationTime);
            IsUsed = false;
            IsRevoked = false;
        }

        private UserRefreshToken(Guid id,
            Guid userId,
            string refreshToken,
            DeviceInfo deviceInfo,
            string ipAddress,
            string? city,
            DateTime expiresAt,
            DateTime createdAt,
            bool isUsed,
            bool isRevoked)
            : base(id, createdAt)
        {
            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            ExpiresAt = expiresAt;
            IsUsed = isUsed;
            IsRevoked = isRevoked;
        }

        public static UserRefreshToken CreateRefreshToken(Guid userId, string refreshToken, DeviceInfo deviceInfo, string ipAddress, string? city)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new SecurityDomainException(ErrorCodes.TokenError.TokenEmpty, "Токен не може бути порожнім.");

            if (userId == Guid.Empty)
                throw new BusinessRuleViolationDomainException(ErrorCodes.GeneralError.OperationNotAllowed, "Помилка ідентифікації користувача.");

            return new UserRefreshToken(userId, refreshToken, deviceInfo, ipAddress, city);
        }

        public static UserRefreshToken Restore(Guid id,
            Guid userId,
            string refreshToken,
            DeviceInfo deviceInfo,
            string ipAddress,
            string? city,
            DateTime expiresAt,
            DateTime createdAt,
            bool isUsed,
            bool isRevoked)
        {
            return new UserRefreshToken(id, userId, refreshToken, deviceInfo, ipAddress, city, expiresAt, createdAt, isUsed, isRevoked);
        }

        public void Revoke()
        {
            if (IsRevoked) return;
            IsRevoked = true;
        }

        public void Use(DateTime now)
        {
            if (!IsActive(now))
                throw new SecurityDomainException(ErrorCodes.TokenError.TokenInvalid, "Токен вже недійсний");
            IsUsed = true;
        }

        public bool IsActive(DateTime now) => !IsUsed && !IsRevoked && now < ExpiresAt;
    }
}
