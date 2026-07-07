namespace Authorization.Requests.Login
{
    public class LoginWithEmailRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
