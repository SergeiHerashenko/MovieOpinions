namespace Authorization.Requests.Login
{
    public class LoginWithPhoneRequest
    {
        public string CountryCode { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Password {  get; set; } = string.Empty;
    }
}
