using Authorization.Application.Common.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.DTOs.Communication.Contacts.Requests
{
    public sealed class DeleteContactRequest<TId>
    {
        public TId UserId { get; }

        public string ContactValue { get; }

        public CommunicationChannel Channel { get; }

        internal DeleteContactRequest(
            TId userId,
            string contactValue,
            CommunicationChannel communicationChannel)
        {
            UserId = userId;
            ContactValue = contactValue;
            Channel = communicationChannel;
        }
    }

    public static class DeleteContactRequest
    {
        public static DeleteContactRequest<TId> Create<TId>(
            AggregateRootId<TId> aggregateId,
            Login login,
            CommunicationChannel channel)
        {
            return new(aggregateId.Value, login.Value, channel);
        }

    }
}
