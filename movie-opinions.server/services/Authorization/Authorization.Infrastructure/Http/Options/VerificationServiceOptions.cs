namespace Authorization.Infrastructure.Http.Options
{
    public class VerificationServiceOptions
    {
        public const string SectionName = "VerificationService";

        public string ClientName { get; set; } = string.Empty;

        public string CreateEndpoint { get; set; } = string.Empty;
    }
}
