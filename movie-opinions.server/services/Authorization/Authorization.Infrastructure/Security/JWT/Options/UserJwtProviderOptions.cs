namespace Authorization.Infrastructure.Security.JWT.Options
{
    public class UserJwtProviderOptions
    {
        public const string SectionName = "Authentication:UserJwt";

        public string Key { get; init; } = string.Empty;

        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        public int AccessTokenLifetimeInMinutes { get; init; } = 15;
    }
}
