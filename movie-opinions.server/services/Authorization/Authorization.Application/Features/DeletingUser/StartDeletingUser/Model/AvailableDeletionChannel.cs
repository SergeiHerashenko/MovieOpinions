using Authorization.Application.Common.Enums;

namespace Authorization.Application.Features.DeletingUser.StartDeletingUser.Model
{
    public class AvailableDeletionChannel
    {
        public Guid ContactId { get; }

        public CommunicationChannel Channel { get; }

        public string MaskedValue { get; }

        public AvailableDeletionChannel(
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
