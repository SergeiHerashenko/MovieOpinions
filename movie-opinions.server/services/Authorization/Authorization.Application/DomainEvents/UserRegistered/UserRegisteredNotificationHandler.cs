using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Events;
using Authorization.Application.DTOs.Communication;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Enums;
using MediatR;

namespace Authorization.Application.DomainEvents.UserRegistered
{
    public sealed class UserRegisteredNotificationHandler 
        : INotificationHandler<DomainEventNotification<UserRegisteredEvent>>
    {
        private readonly INotificationSender _notificationSender;

        public UserRegisteredNotificationHandler(INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;
        }

        public async Task Handle(
            DomainEventNotification<UserRegisteredEvent> notification, 
            CancellationToken cancellationToken = default)
        {
            var domainEvent = notification.DomainEvent;

            var channel = domainEvent.Login.Type == LoginType.Email
                ? CommunicationChannel.Email
                : CommunicationChannel.Phone;

            var notificationCommand = NotificationRequest.Create(
                domainEvent.UserId, 
                domainEvent.Login,
                NotificationType.ConfirmRegistration, 
                channel
            );

            await _notificationSender.SendCreateNotificationAsync(notificationCommand, cancellationToken);
        }
    }
}
