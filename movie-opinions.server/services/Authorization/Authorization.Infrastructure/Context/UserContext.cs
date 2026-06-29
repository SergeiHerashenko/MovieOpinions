using Authorization.Application.Interfaces.Context;
using Microsoft.AspNetCore.Http;

namespace Authorization.Infrastructure.Context
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return "unknown";

            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();

            if (!string.IsNullOrEmpty(realIp))
                return realIp;

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        public string GetLanguage()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return "en";

            // Перевіряємо кастомний заголовок
            var customLang = context.Request.Headers["X-Language"].FirstOrDefault();
            if (!string.IsNullOrEmpty(customLang))
            {
                return customLang.Split(',')[0].Split(';')[0].Trim().ToLower();
            }

            // Перевіряємо стандартний заголовок Accept-Language
            var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();
            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                return acceptLanguage.Split(',')[0].Split(';')[0].Trim().ToLower();
            }

            return "en";
        }
    }
}
