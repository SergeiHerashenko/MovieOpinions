using Authorization.Domain.UsersRefreshToken.ValueObjects;

namespace Authorization.Application.Interfaces.Context
{
    public interface IUserContext
    {
        DeviceInfo DeviceInfo { get; }

        string GetLanguage();

        string GetIpAddress();

        string? GetLocation();
    }
}
