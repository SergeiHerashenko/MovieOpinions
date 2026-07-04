using Authorization.Application.Interfaces.Context;
using Authorization.Domain.UsersRefreshToken.ValueObjects;
using Microsoft.AspNetCore.Http;
using System.Net;
using UAParser;

namespace Authorization.Infrastructure.Context
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly Parser _auParser = Parser.GetDefault();

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DeviceInfo DeviceInfo
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;

                if (context is null)
                    return DeviceInfo.Create("Unknown", "Unknown", "Unknown", "Unknown").Value;

                var userAgentRaw = context.Request.Headers["User-Agent"].ToString();

                if (string.IsNullOrWhiteSpace(userAgentRaw))
                    return DeviceInfo.Create("Unknown", "Unknown", "Unknown", "Unknown").Value;

                ClientInfo client = _auParser.Parse(userAgentRaw);

                string browser = client.UA.Family;
                string os = client.OS.Family;

                string deviceType;
                string deviceModel;

                if (client.Device.Family == "Other")
                {
                    deviceType = "Desktop";
                    deviceModel = "Desktop";
                }
                else
                {
                    deviceType = userAgentRaw.Contains("iPad", StringComparison.OrdinalIgnoreCase) ||
                                 userAgentRaw.Contains("Tablet", StringComparison.OrdinalIgnoreCase)
                                 ? "Tablet" : "Mobile";

                    deviceModel = client.Device.Model ?? client.Device.Family;
                }

                return DeviceInfo.Create(deviceType, os, browser, deviceModel).Value;
            }
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

        public string? GetLocation()
        {
            var context = _httpContextAccessor.HttpContext;

            if(context is null)
                return null;

            var rawLocation = context.Request.Headers["X-User-City"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(rawLocation))
                return null;

            try
            {
                return WebUtility.UrlDecode(rawLocation);
            }
            catch
            {
                return null;
            }
        }
    }
}
