using Authorization.Application.Common.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.DTOs.Communication
{
    public class NotificationRequest<TId>
    {
        public TId UserId { get; }

        public string Recipient {  get; }

        public MessageActions Action { get; }

        public CommunicationChannel Channel { get; }

        internal NotificationRequest(
            TId userId,
            string recipient, 
            MessageActions action,
            CommunicationChannel channel)
        {
            UserId = userId;
            Recipient = recipient;
            Action = action;
            Channel = channel;
        }
    }

    public static class NotificationRequest
    {
        public static NotificationRequest<TId> Create<TId>(
            AggregateRootId<TId> aggregateId,
            Login recipient,
            MessageActions messageActions)
        {
            if (recipient.Type == LoginType.Email)
                return new(aggregateId.Value, recipient.Value, messageActions, CommunicationChannel.Email);

            return new(aggregateId.Value, recipient.Value, messageActions, CommunicationChannel.Phone);
        }
    }
}
