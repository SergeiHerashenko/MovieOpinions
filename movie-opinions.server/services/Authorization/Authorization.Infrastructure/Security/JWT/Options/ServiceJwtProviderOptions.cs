namespace Authorization.Infrastructure.Security.JWT.Options
{
    public class ServiceJwtProviderOptions
    {
        public const string SectionName = "Authentication:ServiceJwt";

        public string Key { get; init; } = string.Empty;

        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;
    }
}
