using Authorization.Application.Common.Enums;
using Authorization.Application.DTOs.Communication.Notifications.Enums;

namespace Authorization.Application.Features.Services.UserActionConfirmation.Mappers.Models
{
    public sealed class ActionNotificationConfig
    {
        public RateLimitAction RateLimitAction { get; }

        public NotificationType NotificationType { get; }

        public ActionNotificationConfig(
            RateLimitAction rateLimitAction, 
            NotificationType notificationType)
        {
            RateLimitAction = rateLimitAction;
            NotificationType = notificationType;
        }
    }
}
