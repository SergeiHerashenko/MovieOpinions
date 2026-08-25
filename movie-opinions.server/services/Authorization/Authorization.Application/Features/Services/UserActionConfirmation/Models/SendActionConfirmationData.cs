
namespace Authorization.Application.Features.Services.UserActionConfirmation.Models
{
    public sealed class SendActionConfirmationData
    {
        public string ContactId { get; }

        public string ConfirmationToken { get; }

        public string MaskedValue { get; }

        public SendActionConfirmationData(
            string contactId,
            string confirmationToken,
            string maskedValue)
        {
            ContactId = contactId;
            ConfirmationToken = confirmationToken;
            MaskedValue = maskedValue;
        }
    }
}
