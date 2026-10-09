using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Errors;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Validation
{
    internal static partial class DeviceInfoValidator
    {
        /// <summary>
        /// Перевіряє, що довжина обов’язкових текстових полів DeviceInfo
        /// не перевищує встановлені обмеження.
        ///
        /// Правило повинно виконуватися лише після успішної перевірки
        /// наявності всіх текстових значень правилом
        /// <see cref="RequiredFieldsRule"/>.
        ///
        /// (Validates that required DeviceInfo text fields
        /// do not exceed their configured length limits.
        ///
        /// The rule must only execute after the presence of all text values
        /// has been successfully validated by
        /// <see cref="RequiredFieldsRule"/>.)
        /// </summary>
        private sealed class FieldLengthsRule : IValidationRule<DeviceInfoValidationData, ValidationFailure>
        {
            private const int MAX_OPERATING_SYSTEM_LENGTH = 100;

            private const int MAX_BROWSER_LENGTH = 100;

            private const int MAX_DEVICE_MODEL_LENGTH = 200;

            public ValidationPriority Priority => ValidationPriority.Length;

            public ValidationFailure? Validate(DeviceInfoValidationData data)
            {
                return ValidateOperatingSystemLength(
                    data.OperationType,
                    data.OperatingSystem
                ) ?? ValidateBrowserLength(
                    data.OperationType,
                    data.Browser
                ) ?? ValidateDeviceModelLength(
                    data.OperationType,
                    data.DeviceModel
                );
            }

            private static ValidationFailure? ValidateOperatingSystemLength(
                OperationType operationType,
                string operatingSystem)
            {
                if (string.IsNullOrWhiteSpace(operatingSystem))
                {
                    throw CreatePreconditionException(
                        nameof(DeviceInfo.OperatingSystem),
                        operationType
                    );
                }

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

            private static ValidationFailure? ValidateBrowserLength(
                OperationType operationType,
                string browser)
            {
                if (string.IsNullOrWhiteSpace(browser))
                {
                    throw CreatePreconditionException(
                        nameof(DeviceInfo.Browser),
                        operationType
                    );
                }

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

            private static ValidationFailure? ValidateDeviceModelLength(
                OperationType operationType,
                string deviceModel)
            {
                if (string.IsNullOrWhiteSpace(deviceModel))
                {
                    throw CreatePreconditionException(
                        nameof(DeviceInfo.DeviceModel),
                        operationType
                    );
                }
                
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

            private static DomainInvalidOperationException CreatePreconditionException(
                string fieldName,
                OperationType operationType)
            {
                return DomainInvalidOperationException.PreconditionFailed<DeviceInfo>(
                    nameof(FieldLengthsRule),
                    nameof(RequiredFieldsRule),
                    operationType,
                    context: new Dictionary<string, object>
                    {
                        ["FieldName"] = fieldName
                    }
                );
            }
        }
    }
}
