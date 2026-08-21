namespace Authorization.Requests.DeletionUser
{
    public class StartDeletionRequest
    {
        public string Password { get; set; } = string.Empty;

        public string? Reason {  get; set; }
    }
}
