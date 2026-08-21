using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Events;
using Authorization.Application.DTOs.Communication;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Enums;
using MediatR;

namespace Authorization.Application.DomainEvents.UserRestrictionSessions
{
    public sealed class UserRestrictionSessionRemovedNotificationHandler
        : INotificationHandler<DomainEventNotification<UserRestrictionSessionRemovedEvent>>
    {
        private readonly INotificationSender _notificationSender;

        public UserRestrictionSessionRemovedNotificationHandler(INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;
        }

        public async Task Handle(
            DomainEventNotification<UserRestrictionSessionRemovedEvent> notification, 
            CancellationToken cancellationToken = default)
        {
            var domainEvent = notification.DomainEvent;

            var channel = domainEvent.Login.Type == LoginType.Email
                ? CommunicationChannel.Email
                : CommunicationChannel.Phone;

            var notificationCommand = NotificationRequest.Create(
                domainEvent.UserRestrictionSessionId,
                domainEvent.Login,
                NotificationType.RemoveSession,
                channel,
                domainEvent.RestrictionType
            );

            await _notificationSender.SendCreateNotificationAsync(notificationCommand, cancellationToken);
        }
    }
}
