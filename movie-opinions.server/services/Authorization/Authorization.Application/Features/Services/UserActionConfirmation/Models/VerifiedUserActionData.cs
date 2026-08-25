namespace Authorization.Application.Features.Services.UserActionConfirmation.Models
{
    public sealed class VerifiedUserActionData
    {
        public string ConfirmationToken { get; }

        public string VerificationValue { get; }

        public VerifiedUserActionData(
            string confirmationToken,
            string verificationValue)
        {
            ConfirmationToken = confirmationToken;
            VerificationValue = verificationValue;
        }
    }
}
