using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Events;
using Authorization.Application.DomainEvents.UserRestrictionSessions.Data;
using Authorization.Application.DTOs.Communication;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Enums;
using MediatR;

namespace Authorization.Application.DomainEvents.UserRestrictionSessions
{
    public sealed class RestrictionAddedToSessionNotificationHandler 
        : INotificationHandler<DomainEventNotification<UserRestrictionSessionAddRestrictionEvent>>
    {
        private readonly INotificationSender _notificationSender;

        public RestrictionAddedToSessionNotificationHandler(INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;
        }

        public async Task Handle(
            DomainEventNotification<UserRestrictionSessionAddRestrictionEvent> notification, 
            CancellationToken cancellationToken = default)
        {
            var domainEvent = notification.DomainEvent;

            var channel = domainEvent.Login.Type == LoginType.Email
                ? CommunicationChannel.Email
                : CommunicationChannel.Phone;

            var restrictionItems = domainEvent.RestrictionDescription
                .Select(x => new RestrictionDetails(x.Rule.Name, x.Rule.DurationMinute, x.Reason))
                .ToList();

            var sessionData = new SessionRestrictionNotificationData(
                restrictionItems, 
                domainEvent.TotalBlockedMinutes,
                domainEvent.RestrictionType.ToString()
            );

            var notificationCommand = NotificationRequest.Create(
                domainEvent.UserRestrictionSessionId,
                domainEvent.Login,
                NotificationType.AddRestrictionSession,
                channel,
                sessionData
            );

            await _notificationSender.SendCreateNotificationAsync(notificationCommand, cancellationToken);
        }
    }
}
