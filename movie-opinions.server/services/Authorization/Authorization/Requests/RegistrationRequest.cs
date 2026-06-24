namespace Authorization.Requests
{
    public class RegistrationRequest
    {
        public string Login { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        public bool AcceptTerms { get; set; }
    }
}
