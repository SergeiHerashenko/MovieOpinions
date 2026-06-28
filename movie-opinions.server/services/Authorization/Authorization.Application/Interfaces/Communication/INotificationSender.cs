using Authorization.Application.DTOs.Communication;
using Authorization.Application.Result;

namespace Authorization.Application.Interfaces.Communication
{
    public interface INotificationSender
    {
        Task<ApplicationResult> SendCreateNotificationAsync(NotificationCommand notificationCommand);
    }
}
