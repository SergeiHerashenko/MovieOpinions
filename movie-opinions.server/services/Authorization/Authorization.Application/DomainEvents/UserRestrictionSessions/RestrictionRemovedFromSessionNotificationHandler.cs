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
    public sealed class RestrictionRemovedFromSessionNotificationHandler
        : INotificationHandler<DomainEventNotification<UserRestrictionSessionRemovedRestrictionEvent>>
    {
        private readonly INotificationSender _notificationSender;

        public RestrictionRemovedFromSessionNotificationHandler(INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;
        }

        public async Task Handle(
            DomainEventNotification<UserRestrictionSessionRemovedRestrictionEvent> notification,
            CancellationToken cancellationToken = default)
        {
            var domainEvent = notification.DomainEvent;

            var channel = domainEvent.Login.Type == LoginType.Email
                ? CommunicationChannel.Email
                : CommunicationChannel.Phone;

            var restrictionItem = new RestrictionInfo(
                domainEvent.RestrictionRule.Name,
                domainEvent.RestrictionRule.DurationMinutes
            );

            IReadOnlyCollection<RestrictionInfo> restrictionCollection = [restrictionItem];

            var sessionData = new SessionRestrictionNotificationData(
                restrictionCollection, 
                domainEvent.TotalBlockedMinutes,
                domainEvent.RestrictionType.ToString()
            );

            var notificationCommand = NotificationRequest.Create(
                domainEvent.UserRestrictionSessionId,
                domainEvent.Login,
                NotificationType.RemoveRestrictionSession,
                channel,
                sessionData
            );

            await _notificationSender.SendCreateNotificationAsync(notificationCommand, cancellationToken);
        }
    }
}
