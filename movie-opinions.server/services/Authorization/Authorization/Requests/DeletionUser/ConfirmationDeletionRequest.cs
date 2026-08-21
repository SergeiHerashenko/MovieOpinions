namespace Authorization.Requests.DeletionUser
{
    public class ConfirmationDeletionRequest
    {
        public string ConfirmationToken { get; set; } = string.Empty;

        public string VerificationValue { get; set; } = string.Empty;
    }
}
