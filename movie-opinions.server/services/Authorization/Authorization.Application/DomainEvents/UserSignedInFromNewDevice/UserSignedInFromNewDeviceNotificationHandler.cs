using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Events;
using Authorization.Application.DomainEvents.UserSignedInFromNewDevice.Data;
using Authorization.Application.DTOs.Communication;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Enums;
using MediatR;

namespace Authorization.Application.DomainEvents.UserSignedInFromNewDevice
{
    public sealed class UserSignedInFromNewDeviceNotificationHandler 
        : INotificationHandler<DomainEventNotification<UserSignedInFromNewDeviceEvent>>
    {
        private readonly INotificationSender _notificationSender;

        public UserSignedInFromNewDeviceNotificationHandler(INotificationSender notificationSender)
        {
            _notificationSender = notificationSender;
        }

        public async Task Handle(
            DomainEventNotification<UserSignedInFromNewDeviceEvent> notification,
            CancellationToken cancellationToken = default)
        {
            var domainEvent = notification.DomainEvent;

            var channel = domainEvent.Login.Type == LoginType.Email
                ? CommunicationChannel.Email
                : CommunicationChannel.Phone;

            var newDeviceLoginNotificationData = new NewDeviceLoginNotificationData(
                domainEvent.IpAddress.Value,
                domainEvent.DeviceInfo.DeviceType.ToString(),
                domainEvent.DeviceInfo.OperatingSystem,
                domainEvent.DeviceInfo.Browser,
                domainEvent.DeviceInfo.DeviceModel,
                domainEvent.Now
            );

            var notificationCommand = NotificationRequest.Create(
                domainEvent.UserId, 
                domainEvent.Login,
                NotificationType.NewLogin,
                channel, 
                newDeviceLoginNotificationData
            );

            await _notificationSender.SendCreateNotificationAsync(notificationCommand, cancellationToken);
        }
    }
}
