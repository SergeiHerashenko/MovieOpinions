using Authorization.Application.Common.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.DTOs.Communication
{
    public class ContactsRequest<TId>
    {
        public TId UserId { get; }

        public string ContactValue { get; }

        public CommunicationChannel Channel { get; }

        internal ContactsRequest(
            TId userId,
            string contactValue,
            CommunicationChannel communicationChannel)
        {
            UserId = userId;
            ContactValue = contactValue;
            Channel = communicationChannel;
        }
    }

    public static class ContactsRequest
    {
        public static ContactsRequest<TId> Create<TId>(
            AggregateRootId<TId> aggregateId,
            Login login)
        {
            if (login.Type == LoginType.Email)
                return new(aggregateId.Value, login.Value, CommunicationChannel.Email);

            return new(aggregateId.Value, login.Value, CommunicationChannel.Phone);
        }
            
    }
}
