using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.Common.Security.Models
{
    public class UserSessionDTO
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public Role Role { get; }

        private UserSessionDTO(UserId userId, Login login, Role role)
        {
            UserId = userId;
            Login = login;
            Role = role;
        }

        public static UserSessionDTO Create(UserId userId, Login login, Role role)
        {
            return new(userId, login, role);
        }
    }
}
