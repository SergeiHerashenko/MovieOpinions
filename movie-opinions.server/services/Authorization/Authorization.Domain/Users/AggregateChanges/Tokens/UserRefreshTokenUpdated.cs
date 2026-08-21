using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.AggregateChanges.Tokens
{
    public sealed class UserRefreshTokenUpdated : AggregateChange
    {
        public UserRefreshTokenId RefreshTokenId { get; }

        public UserId UserId { get; }

        public TokenStatus TokenStatus { get; }

        public DateTimeOffset? ConsumedAt { get; }

        public DateTimeOffset? RevokedAt { get; }

        public DateTimeOffset Now { get; }

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
