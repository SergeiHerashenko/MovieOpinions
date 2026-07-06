namespace Authorization.Infrastructure.Integration.Options
{
    public class ProfileServiceOption
    {
        public const string SectionName = "Profile";

        public string ClientName { get; set; } = string.Empty;

        public string CreateEndpoint { get; set; } = string.Empty;
    }
}
