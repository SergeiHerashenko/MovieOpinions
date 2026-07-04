using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersRefreshToken.ValueObjects;

namespace Authorization.Application.Common.Security.Models
{
    public class UserSessionDTO
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public Role Role { get; }

        public IpAddress IpAddress { get; }

        private UserSessionDTO(UserId userId, Login login, Role role, IpAddress ipAddress)
        {
            UserId = userId;
            Login = login;
            Role = role;
            IpAddress = ipAddress;
        }

        public static UserSessionDTO Create(UserId userId, Login login, Role role, IpAddress ipAddress)
        {
            return new(userId, login, role, ipAddress);
        }
    }
}
