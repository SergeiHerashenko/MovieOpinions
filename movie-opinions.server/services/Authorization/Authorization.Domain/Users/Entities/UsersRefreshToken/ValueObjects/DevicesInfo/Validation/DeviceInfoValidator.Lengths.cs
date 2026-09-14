using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Errors;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Validation
{
    internal static partial class DeviceInfoValidator
    {
        /// <summary>
        /// Перевіряє, що довжина текстових полів DeviceInfo
        /// не перевищує встановлені обмеження.
        ///
        /// (Validates that DeviceInfo text fields
        /// do not exceed their configured length limits.)
        /// </summary>
        private sealed class FieldLengthsRule : IValidationRule<DeviceInfoValidationData, ValidationFailure>
        {
            private const int MAX_OPERATING_SYSTEM_LENGTH = 100;

            private const int MAX_BROWSER_LENGTH = 100;

            private const int MAX_DEVICE_MODEL_LENGTH = 200;

            public ValidationPriority Priority => ValidationPriority.Length;

            public ValidationFailure? Validate(DeviceInfoValidationData value)
            {
                return ValidateOperatingSystemLength(value.OperatingSystem)
                    ?? ValidateBrowserLength(value.Browser)
                    ?? ValidateDeviceModelLength(value.DeviceModel);
            }

            private static ValidationFailure? ValidateOperatingSystemLength(string? operatingSystem)
            {
                if (string.IsNullOrWhiteSpace(operatingSystem))
                    return null;

                if (operatingSystem.Length <= MAX_OPERATING_SYSTEM_LENGTH)
                    return null;

                return CreateFailure(
                    nameof(DeviceInfo.OperatingSystem),
                    operatingSystem.Length,
                    MAX_OPERATING_SYSTEM_LENGTH,
                    DeviceInfoErrors.TooLongOperatingSystem<DeviceInfo>(
                        operatingSystem.Length,
                        MAX_OPERATING_SYSTEM_LENGTH
                    )
                );
            }

            private static ValidationFailure? ValidateBrowserLength(string? browser)
            {
                if (string.IsNullOrWhiteSpace(browser))
                    return null;

                if (browser.Length <= MAX_BROWSER_LENGTH)
                    return null;

                return CreateFailure(
                    nameof(DeviceInfo.Browser),
                    browser.Length,
                    MAX_BROWSER_LENGTH,
                    DeviceInfoErrors.TooLongBrowser<DeviceInfo>(
                        browser.Length,
                        MAX_BROWSER_LENGTH
                    )
                );
            }

            private static ValidationFailure? ValidateDeviceModelLength(string? deviceModel)
            {
                if (string.IsNullOrWhiteSpace(deviceModel))
                    return null;

                if (deviceModel.Length <= MAX_DEVICE_MODEL_LENGTH)
                    return null;

                return CreateFailure(
                    nameof(DeviceInfo.DeviceModel),
                    deviceModel.Length,
                    MAX_DEVICE_MODEL_LENGTH,
                    DeviceInfoErrors.TooLongDeviceModel<DeviceInfo>(
                        deviceModel.Length,
                        MAX_DEVICE_MODEL_LENGTH
                    )
                );
            }

            private static ValidationFailure CreateFailure(
                string fieldName,
                int actualLength,
                int maximumLength,
                Error error)
            {
                return new ValidationFailure()
                {
                    Error = error,
                    BuildException = operationType => DomainDataInconsistencyException.ValueOutOfRange<DeviceInfo>(
                        fieldName,
                        actualLength,
                        operationType,
                        context: new Dictionary<string, object>
                        {
                            ["ActualLength"] = actualLength,
                            ["MaximumLength"] = maximumLength
                        }
                    )
                };
            }
        }
    }
}
