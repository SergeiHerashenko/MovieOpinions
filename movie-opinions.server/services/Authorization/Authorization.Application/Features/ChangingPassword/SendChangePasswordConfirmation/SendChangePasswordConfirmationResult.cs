using Authorization.Application.Common.Enums;

namespace Authorization.Application.Features.ChangingPassword.SendChangePasswordConfirmation
{
    public sealed class SendChangePasswordConfirmationResult
    {
        public ConfirmationNextStep ConfirmationNextStep { get; }

        public string Message { get; }

        public SendChangePasswordConfirmationResult(
            ConfirmationNextStep confirmationNextStep,
            string message)
        {
            ConfirmationNextStep = confirmationNextStep;
            Message = message;
        }
    }
}
