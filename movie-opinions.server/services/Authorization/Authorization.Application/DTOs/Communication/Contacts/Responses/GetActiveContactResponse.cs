using Authorization.Application.Common.Enums;

namespace Authorization.Application.DTOs.Communication.Contacts.Responses
{
    public sealed class GetActiveContactResponse
    {
        public Guid ContactId { get; }

        public string ContactValue {  get; }

        public CommunicationChannel Channel { get; }

        public GetActiveContactResponse(
            Guid contactId,
            string contactValue,
            CommunicationChannel channel)
        {
            ContactId = contactId;
            ContactValue = contactValue;
            Channel = channel;
        }
    }
}
