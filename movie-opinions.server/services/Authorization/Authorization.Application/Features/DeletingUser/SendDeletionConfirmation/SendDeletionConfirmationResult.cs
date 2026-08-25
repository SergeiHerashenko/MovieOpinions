using Authorization.Application.Common.Enums;

namespace Authorization.Application.Features.DeletingUser.SendDeletionConfirmation
{
    public sealed class SendDeletionConfirmationResult
    {
        public ConfirmationNextStep ConfirmationNextStep { get; }
        
        public string Message { get; }

        public SendDeletionConfirmationResult(
            ConfirmationNextStep confirmationNextStep, 
            string message)
        {
            ConfirmationNextStep = confirmationNextStep;
            Message = message;
        }
    }
}
