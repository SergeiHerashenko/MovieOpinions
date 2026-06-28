namespace Authorization.Requests
{
    public class RegistrationWithPhoneRequest
    {
        public string CountryCode { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        public bool AcceptTerms { get; set; }
    }
}
