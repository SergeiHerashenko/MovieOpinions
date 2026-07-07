namespace Authorization.Cookie
{
    public class CookieSettingsOptions
    {
        public string AccessTokenName { get; init; } = string.Empty;

        public string RefreshTokenName { get; init; } = string.Empty;

        public int AccessTokenExpirationMinutes { get; init; }

        public int RefreshTokenExpirationDays { get; init; } 
    }
}
