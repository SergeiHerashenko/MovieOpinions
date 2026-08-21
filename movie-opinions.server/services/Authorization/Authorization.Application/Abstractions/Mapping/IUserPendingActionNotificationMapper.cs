using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;

namespace Authorization.Application.Abstractions.Mapping
{
    public interface IUserPendingActionNotificationMapper
    {
        NotificationType ToMessageActionType(UserAction userAction);
    }
}
