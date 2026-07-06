using Authorization.Application.Common.Enums;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.Features.Authentication.ConfirmRegistration
{
    public class ConfirmRegistrationContext
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public Role Role { get; }

        public MessageActions MessageActions { get; }

        private ConfirmRegistrationContext(
            UserId userId,
            Login login,
            Role role,
            MessageActions messageActions)
        {
            UserId = userId;
            Login = login;
            Role = role;
            MessageActions = messageActions;
        }

        public static ConfirmRegistrationContext Create(
            UserId userId,
            Login login,
            Role role,
            MessageActions messageActions)
            => new(userId, login, role, messageActions);
    }
}
