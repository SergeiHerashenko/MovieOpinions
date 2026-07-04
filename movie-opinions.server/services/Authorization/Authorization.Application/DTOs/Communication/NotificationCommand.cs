using Authorization.Application.Common.Enums;

namespace Authorization.Application.DTOs.Communication
{
    public class NotificationCommand
    {
        public string Recipient {  get; }

        public MessageActions Action { get; }

        public CommunicationChannel Channel { get; }

        private NotificationCommand(
            string recipient, 
            MessageActions action,
            CommunicationChannel channel)
        {
            Recipient = recipient;
            Action = action;
            Channel = channel;
        }

        public static NotificationCommand Create(string recipient, MessageActions action, CommunicationChannel channel)
            => new(recipient, action, channel);
    }
}
