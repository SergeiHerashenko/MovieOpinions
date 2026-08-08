using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Events;
using Authorization.Application.DTOs.Communication;
using Authorization.Domain.UsersPendingRegistration.DomainEvents;
using MediatR;

namespace Authorization.Application.DomainEvents.UserPendingRegistration
{
    public class SendNotificationOnRegistrationRequested : INotificationHandler<DomainEventNotification<UserPendingRegistrationEvent>>
    {
        private readonly INotificationSender _notificationSender;

        public SendNotificationOnRegistrationRequested(INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;
        }

        public async Task Handle(
            DomainEventNotification<UserPendingRegistrationEvent> notification,
            CancellationToken cancellationToken = default)
        {
            var domainEvent = notification.DomainEvent;

            var notificationCommand = NotificationRequest.Create(domainEvent.Id, domainEvent.Login, MessageActions.Registration);

            await _notificationSender.SendCreateNotificationAsync(notificationCommand);
        }
    }
}
