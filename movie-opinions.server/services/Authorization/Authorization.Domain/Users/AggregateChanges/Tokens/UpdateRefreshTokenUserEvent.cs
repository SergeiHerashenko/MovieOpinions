using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRefreshToken;

namespace Authorization.Domain.Users.AggregateChanges.Tokens
{
    public sealed class UpdateRefreshTokenUserEvent : AggregateChange
    {
        public UserRefreshToken UserRefreshToken { get; }

        public DateTimeOffset Now { get; }

        public UpdateRefreshTokenUserEvent(UserRefreshToken userRefreshToken, DateTimeOffset now)
            : base(now)
        {
            UserRefreshToken = userRefreshToken;
            Now = now;
        }
    }
}
