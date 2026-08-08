using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRefreshToken;

namespace Authorization.Domain.Users.AggregateChanges.Tokens
{
    public sealed class UserRefreshTokenUpdated : AggregateChange
    {
        public UserRefreshToken UserRefreshToken { get; }

        public DateTimeOffset Now { get; }

        public UserRefreshTokenUpdated(UserRefreshToken userRefreshToken, DateTimeOffset now)
            : base(now)
        {
            UserRefreshToken = userRefreshToken;
            Now = now;
        }
    }
}
