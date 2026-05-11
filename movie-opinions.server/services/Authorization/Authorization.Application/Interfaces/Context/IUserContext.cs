using Authorization.Domain.ValueObjects;

namespace Authorization.Application.Interfaces.Context
{
    public interface IUserContext
    {
        string? GetUserLogin();

        Guid? GetUserId();

        Guid? GetResetEventId();

        string GetIpAddress();

        string GetUserAgent();

        DeviceInfo GetDevuceInfo();

        string GetLanguage();
    }
}
