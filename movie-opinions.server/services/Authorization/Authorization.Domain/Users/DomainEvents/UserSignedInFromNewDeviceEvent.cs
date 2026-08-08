using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users.DomainEvents
{
    public class UserSignedInFromNewDeviceEvent : DomainEvent
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public DeviceInfo DeviceInfo { get; }

        public IpAddress IpAddress { get; }

        public DateTimeOffset Now { get; }

        public UserSignedInFromNewDeviceEvent(UserId userId, Login login, DeviceInfo deviceInfo, IpAddress ipAddress, DateTimeOffset now)
            : base(now)
        {
            UserId = userId;
            Login = login;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            Now = now;
        }
    }
}
