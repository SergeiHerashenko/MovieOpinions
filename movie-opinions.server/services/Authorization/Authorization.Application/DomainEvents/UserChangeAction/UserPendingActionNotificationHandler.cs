using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Abstractions.Mapping;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Events;
using Authorization.Application.DTOs.Communication;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Enums;
using MediatR;

namespace Authorization.Application.DomainEvents.UserChangeAction
{
    public sealed class UserPendingActionNotificationHandler 
        : INotificationHandler<DomainEventNotification<UserPendingActionEvent>>
    {
        private readonly INotificationSender _notificationSender;
        private readonly IUserPendingActionNotificationMapper _userPendingActionNotificationMapper;

        public UserPendingActionNotificationHandler(
            INotificationSender notificationSender,
            IUserPendingActionNotificationMapper userPendingActionNotificationMapper)
        {
            _notificationSender = notificationSender;
            _userPendingActionNotificationMapper = userPendingActionNotificationMapper;
        }

        public async Task Handle(
            DomainEventNotification<UserPendingActionEvent> notification,
            CancellationToken cancellationToken = default)
        {
            var domainEvent = notification.DomainEvent;

            var channel = domainEvent.Login.Type == LoginType.Email
                ? CommunicationChannel.Email
                : CommunicationChannel.Phone;

            var messageAction = _userPendingActionNotificationMapper.ToMessageActionType(domainEvent.UserAction);

            var notificationCommand = NotificationRequest.Create(
                domainEvent.UserPendingActionId, 
                domainEvent.Login, 
                messageAction, 
                channel
            );

            await _notificationSender.SendCreateNotificationAsync(notificationCommand, cancellationToken);
        }
    }
}
