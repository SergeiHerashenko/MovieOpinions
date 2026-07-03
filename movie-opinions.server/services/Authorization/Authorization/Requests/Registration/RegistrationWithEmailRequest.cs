namespace Authorization.Requests.Registration
{
    public class RegistrationWithEmailRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password {  get; set; } = string.Empty;

        public string ConfirmPassword {  get; set; } = string.Empty;

        public bool AcceptTerms { get; set; }
    }
}
