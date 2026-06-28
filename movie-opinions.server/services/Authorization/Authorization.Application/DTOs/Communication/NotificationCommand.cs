using Authorization.Application.Common.Enums;

namespace Authorization.Application.DTOs.Communication
{
    public class NotificationCommand
    {
        public required string Recipient {  get; set; }

        public MessageActions Action { get; set; }

        public CommunicationChannel Channel { get; set; }
    }
}
