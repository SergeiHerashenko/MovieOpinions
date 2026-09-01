using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Events;
using Authorization.Application.DTOs.Communication;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.UsersPendingRegistration.DomainEvents;
using MediatR;

namespace Authorization.Application.DomainEvents.UserPendingRegistration
{
    public sealed class UserPendingRegistrationNotificationHandler 
        : INotificationHandler<DomainEventNotification<UserRegistrationRequestedEvent>>
    {
        private readonly INotificationSender _notificationSender;

        public UserPendingRegistrationNotificationHandler(INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;
        }

        public async Task Handle(
            DomainEventNotification<UserRegistrationRequestedEvent> notification,
            CancellationToken cancellationToken = default)
        {
            var domainEvent = notification.DomainEvent;

            var channel = domainEvent.Login.Type == LoginType.Email
                ? CommunicationChannel.Email
                : CommunicationChannel.Phone;

            var notificationCommand = NotificationRequest.Create(
                domainEvent.Id, 
                domainEvent.Login,
                NotificationType.StartRegistration, 
                channel
            );

            await _notificationSender.SendCreateNotificationAsync(notificationCommand, cancellationToken);
        }
    }
}
