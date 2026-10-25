using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents
{
    /// <summary>
    /// Доменна подія, що фіксує успішний вхід користувача
    /// з пристрою, який система визначила як новий.
    ///
    /// (Domain event indicating that the user successfully signed in
    /// from a device identified by the system as new.)
    /// </summary>
    public sealed class UserSignedInFromNewDeviceEvent : DomainEvent
    {
        public UserId UserId { get; }

        public Login Login { get; }

        public DeviceInfo DeviceInfo { get; }

        public IpAddress IpAddress { get; }

        /// <summary>
        /// Створює подію успішного входу користувача
        /// з пристрою, який система визначила як новий.
        ///
        /// (Creates an event representing a successful user login
        /// from a device identified by the system as new.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача, який виконав вхід.
        ///
        /// (Identifier of the user who signed in.)
        /// </param>
        /// <param name="login">
        /// Логін користувача.
        ///
        /// (The user login.)
        /// </param>
        /// <param name="deviceInfo">
        /// Інформація про пристрій, з якого виконано вхід.
        ///
        /// (Information about the device used to sign in.)
        /// </param>
        /// <param name="ipAddress">
        /// IP-адреса, з якої виконано вхід.
        ///
        /// (The IP address from which the user signed in.)
        /// </param>
        /// <param name="occurredOn">
        /// Час успішного входу.
        ///
        /// (The successful login time.)
        /// </param>
        public UserSignedInFromNewDeviceEvent(
            UserId userId,
            Login login,
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserId = userId;
            Login = login;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
        }
    }
}
