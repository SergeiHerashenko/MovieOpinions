namespace Authorization.Infrastructure.Http.Options
{
    public class NotificationServiceOptions
    {
        public const string SectionName = "NotificationService";

        public string ClientName { get; set; } = string.Empty;

        public string CreateEndpoint { get; set; } = string.Empty;
    }
}
