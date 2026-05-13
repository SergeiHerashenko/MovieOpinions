using Authorization.Application.Interfaces.Context;
using Authorization.Domain.ValueObjects;
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

        public DeviceInfo GetDevuceInfo()
        {
            throw new NotImplementedException();
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

            var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();
            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                return acceptLanguage
                    .Split(',')
                    .First()
                    .Split('-')[0]
                    .Trim();
            }

            return "en";
        }

        public Guid? GetResetEventId()
        {
            throw new NotImplementedException();
        }

        public string GetUserAgent()
        {
            throw new NotImplementedException();
        }

        public Guid? GetUserId()
        {
            throw new NotImplementedException();
        }

        public string? GetUserLogin()
        {
            throw new NotImplementedException();
        }
    }
}
