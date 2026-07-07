using Microsoft.Extensions.Options;

namespace Authorization.Cookie
{
    public class CookieProvider : ICookieProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CookieSettingsOptions _options;

        public CookieProvider(
            IHttpContextAccessor httpContextAccessor,
            IOptions<CookieSettingsOptions> options)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        public void SetCookies(string accessToken, string refreshToken)
        {
            var responce = _httpContextAccessor.HttpContext?.Response;

            if (responce is null)
                return;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            };

            responce.Cookies.Append(_options.AccessTokenName, accessToken,
                new CookieOptions(cookieOptions) { Expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes) });

            responce.Cookies.Append(_options.RefreshTokenName, refreshToken,
                new CookieOptions(cookieOptions) { Expires = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays) });
        }

        public void ClearCookies()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1)
            };

            context.Response.Cookies.Delete(_options.AccessTokenName, cookieOptions);
            context.Response.Cookies.Delete(_options.RefreshTokenName, cookieOptions);
        }
    }
}
