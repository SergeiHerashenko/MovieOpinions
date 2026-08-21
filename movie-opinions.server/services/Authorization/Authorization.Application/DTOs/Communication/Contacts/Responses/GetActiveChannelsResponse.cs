using Authorization.Application.Common.Enums;

namespace Authorization.Application.DTOs.Communication.Contacts.Responses
{
    public sealed class GetActiveChannelsResponse
    {
        public IReadOnlyCollection<ActiveContactChannelResponse> Channels { get; }

        public GetActiveChannelsResponse(IReadOnlyCollection<ActiveContactChannelResponse> channels)
        {
            Channels = channels;
        }
    }

    public sealed class ActiveContactChannelResponse
    {
        public Guid ContactId { get; }

        public CommunicationChannel Channel { get; }

        public string MaskedValue { get; }

        public ActiveContactChannelResponse(
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
