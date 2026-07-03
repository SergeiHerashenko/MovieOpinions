using Authorization.Application.DTOs.Communication;
using Authorization.Domain.Results;

namespace Authorization.Application.Interfaces.Communication
{
    public interface INotificationSender
    {
        Task<Result> SendCreateNotificationAsync(NotificationCommand notificationCommand);
    }
}
