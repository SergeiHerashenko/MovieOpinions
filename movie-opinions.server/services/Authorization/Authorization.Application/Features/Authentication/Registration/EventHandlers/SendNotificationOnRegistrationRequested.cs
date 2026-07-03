using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Events;
using Authorization.Application.Common.Exceptions;
using Authorization.Application.DTOs.Communication;
using Authorization.Application.Interfaces.Communication;
using Authorization.Domain.DomainEvents.UserPendingRegistration;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using MediatR;

namespace Authorization.Application.Features.Authentication.Registration.EventHandlers
{
    public class SendNotificationOnRegistrationRequested : INotificationHandler<DomainEventNotification<UserPendingRegistrationRequestedEvent>>
    {
        private readonly INotificationSender _notificationSender;

        public SendNotificationOnRegistrationRequested(INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;
        }

        public async Task Handle(
            DomainEventNotification<UserPendingRegistrationRequestedEvent> notification,
            CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            var channel = domainEvent.Login is EmailLogin
                ? CommunicationChannel.Email
                : CommunicationChannel.Phone;

            var notificationCommand = new NotificationCommand
            {
                Recipient = domainEvent.Login.Value,
                Action = MessageActions.Registration,
                Channel = channel
            };

            await _notificationSender.SendCreateNotificationAsync(notificationCommand);
        }
    }
}
