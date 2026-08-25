using Authorization.Application.Common.Enums;

namespace Authorization.Application.Features.Services.UserActiveContacts.Models
{
    public sealed class ActiveContactChannel
    {
        public Guid ContactId { get; }

        public CommunicationChannel Channel { get; }

        public string MaskedValue { get; }

        public ActiveContactChannel(
            Guid contactId, 
            CommunicationChannel channel, 
            string maskedValue)
        {
            ContactId = contactId;
            Channel = channel;
            MaskedValue = maskedValue;
        }
    }
}
