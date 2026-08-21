using Authorization.Application.Common.Enums;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.DTOs.Communication
{
    public class NotificationRequest<TId>
    {
        public TId ReferenceId { get; }

        public string Recipient { get; }

        public NotificationType NotificationType { get; }

        public CommunicationChannel Channel { get; }

        internal NotificationRequest(
            TId userId,
            string recipient,
            NotificationType notificationType,
            CommunicationChannel channel)
        {
            ReferenceId = userId;
            Recipient = recipient;
            NotificationType = notificationType;
            Channel = channel;
        }
    }

    public sealed class NotificationRequest<TId, TData> : NotificationRequest<TId>
    {
        public TData Data { get; }

        internal NotificationRequest(
            TId referenceId,
            string recipient,
            NotificationType notificationType,
            CommunicationChannel channel,
            TData data)
            : base(referenceId, recipient, notificationType, channel)
        {
            Data = data;
        }
    }

    public static class NotificationRequest
    {
        public static NotificationRequest<TId> Create<TId>(
            AggregateRootId<TId> aggregateId,
            Login recipient,
            NotificationType notificationType,
            CommunicationChannel channel)
        {
            return new NotificationRequest<TId>(aggregateId.Value, recipient.Value, notificationType, channel);
        }

        public static NotificationRequest<TId> Create<TId>(
            AggregateRootId<TId> aggregateId,
            string recipient,
            NotificationType notificationType,
            CommunicationChannel channel)
        {
            return new NotificationRequest<TId>(aggregateId.Value, recipient, notificationType, channel);
        }

        public static NotificationRequest<TId, TData> Create<TId, TData>(
            AggregateRootId<TId> aggregateId,
            Login recipient,
            NotificationType notificationType,
            CommunicationChannel channel,
            TData data)
        {
            return new NotificationRequest<TId, TData>(aggregateId.Value, recipient.Value, notificationType, channel, data);
        }
    }
}
