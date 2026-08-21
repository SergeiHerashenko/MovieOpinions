using Authorization.Application.Common.Enums;

namespace Authorization.Application.Features.DeletingUser.SendDeletionConfirmation
{
    public sealed class SendDeletionConfirmationResult
    {
        public ConfirmationNextStep ConfirmationNextStep { get; }
        
        public string MaskedValue { get; }

        public SendDeletionConfirmationResult(ConfirmationNextStep confirmationNextStep, string maskedValue)
        {
            ConfirmationNextStep = confirmationNextStep;
            MaskedValue = maskedValue;
        }
    }
}
