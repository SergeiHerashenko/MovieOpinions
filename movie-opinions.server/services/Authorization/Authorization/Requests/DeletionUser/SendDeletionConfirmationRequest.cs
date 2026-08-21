namespace Authorization.Requests.DeletionUser
{
    public class SendDeletionConfirmationRequest
    {
        public string ConfirmationToken { get; set; } = string.Empty;

        public string ContactId { get; set; } = string.Empty;

        public string MaskedValue { get; set; } = string.Empty;
    }
}
