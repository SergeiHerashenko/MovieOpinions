namespace Authorization.Infrastructure.Communication.Options
{
    public class ProfileServiceOptions
    {
        public const string SectionName = "Profile";

        public string ClientName { get; set; } = string.Empty;

        public string CreateEndpoint { get; set; } = string.Empty;
    }
}
