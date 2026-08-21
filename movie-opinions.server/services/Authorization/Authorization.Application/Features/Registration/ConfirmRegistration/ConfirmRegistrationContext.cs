using Authorization.Application.Common.Enums;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.Features.Registration.ConfirmRegistration
{
    public class ConfirmRegistrationContext
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public Role Role { get; }

        public CommunicationChannel CommunicationChannel { get; }

        public NotificationType NotificationType { get; }

        private ConfirmRegistrationContext(
            UserId userId,
            Login login,
            Role role,
            CommunicationChannel communicationChannel,
            NotificationType notificationType)
        {
            UserId = userId;
            Login = login;
            Role = role;
            CommunicationChannel = communicationChannel;
            NotificationType = notificationType;
        }

        public static ConfirmRegistrationContext Create(
            UserId userId,
            Login login,
            Role role,
            CommunicationChannel communicationChannel,
            NotificationType notificationType)
            => new(userId, login, role, communicationChannel, notificationType);
    }
}
