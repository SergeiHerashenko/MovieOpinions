using Authorization.Domain.Users.Entities.UsersRefreshToken;

namespace Authorization.Application.Common.Security.Models
{
    public class TokenResponse
    {
        public required string AccessToken { get; set; }

        public required UserRefreshToken UserRefreshToken { get; set; }
    }
}
