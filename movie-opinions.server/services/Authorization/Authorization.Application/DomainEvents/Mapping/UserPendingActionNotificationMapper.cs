using Authorization.Application.Abstractions.Mapping;
using Authorization.Application.Common.Exceptions;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;

namespace Authorization.Application.DomainEvents.Mapping
{
    public class UserPendingActionNotificationMapper : IUserPendingActionNotificationMapper
    {
        public NotificationType ToMessageActionType(UserAction userAction)
        {
            return userAction switch
            {
                PasswordChangeAction => NotificationType.PendingChangePassword,
                LoginChangeAction => NotificationType.PendingChangeEmail,
                DeleteAccountAction => NotificationType.PendingDeletingUser,
                _ => throw ApplicationInvalidOperationException.UnsupportedValue<UserPendingActionNotificationMapper>(nameof(userAction))
            };
        }
    }
}
