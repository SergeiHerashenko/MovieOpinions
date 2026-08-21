namespace Authorization.Application.DomainEvents.UserSignedInFromNewDevice.Data
{
    public sealed class NewDeviceLoginNotificationData
    {
        public string IpAddress { get; }

        public string DeviceType { get; }

        public string OperatingSystem { get; }

        public string Browser { get; }

        public string DeviceModel { get; }

        public DateTimeOffset Now { get; }

        public NewDeviceLoginNotificationData(string ipAddress, string deviceType, string operatingSystem, string browser, string deviceModel, DateTimeOffset now)
        {
            IpAddress = ipAddress;
            DeviceType = deviceType;
            OperatingSystem = operatingSystem;
            Browser = browser;
            DeviceModel = deviceModel;
            Now = now;
        }
    }
}
