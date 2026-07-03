namespace Authorization.Requests.ConfirmRegistration
{
    public class ConfirmRegistrationRequest
    {
        public string RegistrationToken { get; set; } = string.Empty;

        public string VerificationValue { get; set; } = string.Empty;
    }
}
