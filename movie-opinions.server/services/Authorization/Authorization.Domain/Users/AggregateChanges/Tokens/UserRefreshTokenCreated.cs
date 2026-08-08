using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRefreshToken;

namespace Authorization.Domain.Users.AggregateChanges.Tokens
{
    public sealed class UserRefreshTokenCreated : AggregateChange
    {
        public UserRefreshToken UserRefreshToken { get; }

        public DateTimeOffset Now { get; }

        public UserRefreshTokenCreated(UserRefreshToken userRefreshToken, DateTimeOffset now)
            : base(now)
        {
            UserRefreshToken = userRefreshToken;
            Now = now;
        }
    }
}
