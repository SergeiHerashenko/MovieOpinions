namespace Authorization.Infrastructure.Communication.Options
{
    public class VerificationServiceOptions
    {
        public const string SectionName = "Verification";

        public string ClientName { get; set; } = string.Empty;

        public string CreateEndpoint { get; set; } = string.Empty;
    }
}
