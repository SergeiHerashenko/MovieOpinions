namespace Authorization.Infrastructure.Integration.Options
{
    public class ServiceIdentityOptions
    {
        public const string SectionName = "ServiceIdentity";

        public string ServiceName { get; set; } = string.Empty;

        public string Scheme { get; set; } = string.Empty;

        public string HeaderName { get; set; } = string.Empty;
    }
}
