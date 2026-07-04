using Authorization.Application.Common.Enums;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.Features.Authentication.ConfirmRegistration
{
    public class ConfirmRegistrationContext
    {
        public UserId UserId { get; }

        public Role Role { get; }

        public LoginType LoginType { get; }

        public MessageActions MessageActions { get; }

        private ConfirmRegistrationContext(
            UserId userId,
            Role role,
            LoginType loginType,
            MessageActions messageActions)
        {
            UserId = userId;
            Role = role;
            LoginType = loginType;
            MessageActions = messageActions;
        }

        public static ConfirmRegistrationContext Create(
            UserId userId,
            Role role,
            LoginType loginType,
            MessageActions messageActions)
            => new(userId, role, loginType, messageActions);
    }
}
