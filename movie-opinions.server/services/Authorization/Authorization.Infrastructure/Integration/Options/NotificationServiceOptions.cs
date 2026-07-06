namespace Authorization.Infrastructure.Integration.Options
{
    public class NotificationServiceOptions
    {
        public const string SectionName = "Notification";

        public string ClientName { get; set; } = string.Empty;

        public string CreateEndpoint { get; set; } = string.Empty;
    }
}
