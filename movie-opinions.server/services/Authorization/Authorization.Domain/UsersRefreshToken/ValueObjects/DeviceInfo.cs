using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using System.Text.Json.Serialization;

namespace Authorization.Domain.UsersRefreshToken.ValueObjects
{
    public class DeviceInfo : ValueObject
    {
        public string DeviceType { get; init; }

        public string OperatingSystem { get; init; }

        public string Browser {  get; init; }

        public string DeviceModel { get; init; }

        [JsonConstructor]
        private DeviceInfo(string deviceType, string operatingSystem, string browser, string deviceModel)
        {
            DeviceType = deviceType;
            OperatingSystem = operatingSystem;
            Browser = browser;
            DeviceModel = deviceModel;
        }

        public static Result<DeviceInfo> Create(string deviceType, string operatingSystem, string browser, string deviceModel)
        {
            return Result<DeviceInfo>.Success(new DeviceInfo(deviceType, operatingSystem, browser, deviceModel));
        }

        public string ToDisplayString()
        {
            var modelInfo = string.IsNullOrEmpty(DeviceModel) ? "" : $"{DeviceModel}, ";
            return $"{modelInfo}{OperatingSystem} ({Browser})";
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return DeviceType;
            yield return OperatingSystem;
            yield return Browser;
            yield return DeviceModel;
        }
    }
}
