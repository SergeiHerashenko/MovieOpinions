using Authorization.Application.Abstractions.Mapping;
using Authorization.Application.Common.Exceptions;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;

namespace Authorization.Application.DomainEvents.Mapping
{
    public class UserActionNotificationMapper : IUserActionNotificationMapper
    {
        public NotificationType ToMessageActionType(UserAction userAction)
        {
            return userAction switch
            {
                PasswordChangeAction => NotificationType.ActionChangePassword,
                LoginChangeAction => NotificationType.ActionChangeLogin,
                DeleteAccountAction => NotificationType.ActionDeletionUser,
                _ => throw ApplicationInvalidOperationException.UnsupportedValue<UserActionNotificationMapper>(nameof(userAction))
            };
        } 
    }
}
