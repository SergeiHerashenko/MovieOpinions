using Authorization.Application.Common.Enums;

namespace Authorization.Application.Features.Services.UserActionConfirmation.Models
{
    public sealed class SendActionConfirmationResult
    {
        public ConfirmationNextStep ConfirmationNextStep { get; }

        public string Message { get; }

        public SendActionConfirmationResult(
            ConfirmationNextStep confirmationNextStep, 
            string message)
        {
            ConfirmationNextStep = confirmationNextStep;
            Message = message;
        }
    }
}
