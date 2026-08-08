using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Rules;
using System.Text.Json.Serialization;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo
{
    public sealed class DeviceInfo : ValueObject
    {
        public DeviceType DeviceType { get; }

        public string OperatingSystem { get; }

        public string Browser { get; }

        public string DeviceModel { get; }

        [JsonConstructor]
        private DeviceInfo(DeviceType deviceType, string operatingSystem, string browser, string deviceModel)
        {
            DeviceType = deviceType;
            OperatingSystem = operatingSystem;
            Browser = browser;
            DeviceModel = deviceModel;
        }

        private static readonly ValidationOrchestrator<DeviceInfoRuleValidationData, ValidationRestoreFailure> _validator = new(
            [
                new EmptyOperatingSystemRule(),
                new EmptyBrowserRule(),
                new EmptyDeviceModelRule()
            ]
        );

        #region Creation 
        public static Result<DeviceInfo> Create(DeviceType deviceType, string operatingSystem, string browser, string deviceModel)
        {
            var failure = ValidateInternal(operatingSystem, browser, deviceModel);

            if (failure is not null)
                return Result<DeviceInfo>.Failure(failure.Error);

            return Result<DeviceInfo>.Success(new DeviceInfo(deviceType, operatingSystem, browser, deviceModel));
        }
        #endregion

        #region Restoration
        public static DeviceInfo Restore(DeviceType deviceType, string operatingSystem, string browser, string deviceModel)
        {
            if (!Enum.IsDefined(typeof(DeviceType), deviceType))
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<DeviceInfo>(nameof(DeviceType), deviceType.ToString());

            var failure = ValidateInternal(operatingSystem, browser, deviceModel);

            if (failure is not null)
                throw failure.BuildException();

            return new(deviceType, operatingSystem, browser, deviceModel);
        }
        #endregion

        private static ValidationRestoreFailure? ValidateInternal(string operatingSystem, string browser, string deviceModel)
        {
            var data = new DeviceInfoRuleValidationData()
            {
                OperatingSystem = operatingSystem,
                Browser = browser,
                DeviceModel = deviceModel
            };

            return _validator.Validate(data);
        }

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return DeviceType;
            yield return OperatingSystem;
            yield return Browser;
            yield return DeviceModel;
        }
    }
}
