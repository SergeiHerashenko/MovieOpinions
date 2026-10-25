using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.AggregateChanges.Tokens
{
    /// <summary>
    /// Фіксує оновлений стан refresh-токена користувача
    /// як зміну агрегату, яку потрібно зберегти.
    ///
    /// (Records the updated user refresh token state
    /// as an aggregate change to be persisted.)
    /// </summary>
    public sealed class UserRefreshTokenUpdated : AggregateChange
    {
        public UserRefreshTokenId RefreshTokenId { get; }

        public UserId UserId { get; }

        public TokenStatus TokenStatus { get; }

        public DateTimeOffset? ConsumedAt { get; }

        public DateTimeOffset? RevokedAt { get; }

        public DateTimeOffset Now { get; }

        /// <summary>
        /// Створює зміну агрегату з оновленим станом refresh-токена.
        ///
        /// (Creates an aggregate change containing the updated refresh token state.)
        /// </summary>
        /// <param name="refreshTokenId">
        /// Ідентифікатор оновленого токена.
        ///
        /// (The updated token identifier.)
        /// </param>
        /// <param name="userId">
        /// Ідентифікатор власника токена.
        ///
        /// (Identifier of the token owner.)
        /// </param>
        /// <param name="tokenStatus">
        /// Оновлений статус токена.
        ///
        /// (The updated token status.)
        /// </param>
        /// <param name="consumedAt">
        /// Час одноразового використання токена або null.
        ///
        /// (The token consumption time, or null.)
        /// </param>
        /// <param name="revokedAt">
        /// Час відкликання токена або null.
        ///
        /// (The token revocation time, or null.)
        /// </param>
        /// <param name="now">
        /// Час зміни стану токена.
        ///
        /// (The time when the token state changed.)
        /// </param>
        public UserRefreshTokenUpdated(
            UserRefreshTokenId refreshTokenId,
            UserId userId,
            TokenStatus tokenStatus,
            DateTimeOffset? consumedAt,
            DateTimeOffset? revokedAt,
            DateTimeOffset now)
            : base(now)
        {
            RefreshTokenId = refreshTokenId;
            UserId = userId;
            TokenStatus = tokenStatus;
            ConsumedAt = consumedAt;
            RevokedAt = revokedAt;
            Now = now;
        }
    }
}
