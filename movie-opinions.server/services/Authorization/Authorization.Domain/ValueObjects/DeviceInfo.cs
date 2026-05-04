using System.Text.Json.Serialization;

namespace Authorization.Domain.ValueObjects
{
    public class DeviceInfo
    {
        public string DeviceType { get; init; }

        public string OperatingSystem { get; init; }

        public string Browser { get; init; }

        public string? DeviceModel { get; init; }

        [JsonConstructor]
        private DeviceInfo(string deviceType, string os, string browser, string? model)
        {
            DeviceType = deviceType;
            OperatingSystem = os;
            Browser = browser;
            DeviceModel = model;
        }

        public static DeviceInfo Create(string type, string os, string browser, string? model = null)
        {
            return new DeviceInfo(type, os, browser, model);
        }

        public string ToDisplayString()
        {
            var modelInfo = string.IsNullOrEmpty(DeviceModel) ? "" : $"{DeviceModel}, ";
            return $"{modelInfo}{OperatingSystem} ({Browser})";
        }
    }
}
