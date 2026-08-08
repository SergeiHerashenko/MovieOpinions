using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;

namespace Authorization.Application.Abstractions.UserContext
{
    public interface IUserContext
    {
        DeviceInfo InfoDevice();

        string GetLanguage();

        string GetIpAddress();

        string? GetLocation();
    }
}
