using Authorization.Application.Common.Enums;
using Authorization.Application.DTOs.Communication.Verification.Enums;

namespace Authorization.Application.Features.Services.UserActionConfirmation.Mappers.Models
{
    public sealed class ActionVerificationConfig
    {
        public RateLimitAction RateLimitAction { get; }

        public VerificationType VerificationType { get; }

        public ActionVerificationConfig(RateLimitAction rateLimitAction, VerificationType verificationType)
        {
            RateLimitAction = rateLimitAction;
            VerificationType = verificationType;
        }
    }
}
