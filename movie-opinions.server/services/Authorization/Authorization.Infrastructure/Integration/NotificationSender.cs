using Authorization.Application.DTOs.Communication;
using Authorization.Application.Interfaces.Communication;
using Authorization.Application.Result;

namespace Authorization.Infrastructure.Integration
{
    public class NotificationSender : INotificationSender
    {
        public async Task<ApplicationResult> SendCreateNotificationAsync(NotificationCommand notificationCommand)
        {
            return ApplicationResult.Success();
        }
    }
}
