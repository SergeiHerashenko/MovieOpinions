namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Rules
{
    public class DeviceInfoRuleValidationData
    {
        public string OperatingSystem { get; set; } = string.Empty;

        public string Browser { get; set; } = string.Empty;

        public string DeviceModel { get; set; } = string.Empty;
    }
}
