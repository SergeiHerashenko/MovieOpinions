using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRefreshToken;

namespace Authorization.Domain.Users.AggregateChanges.Tokens
{
    public sealed class CreateRefreshTokenUserEvent : AggregateChange
    {
        public UserRefreshToken UserRefreshToken { get; }

        public DateTimeOffset Now { get; }

        public CreateRefreshTokenUserEvent(UserRefreshToken userRefreshToken, DateTimeOffset now)
            : base(now)
        {
            UserRefreshToken = userRefreshToken;
            Now = now;
        }
    }
}
