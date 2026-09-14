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
        /// Перевіряє наявність обов’язкових текстових полів DeviceInfo.
        /// Повертає перше виявлене порушення відповідно до порядку перевірок.
        ///
        /// (Validates the presence of required DeviceInfo text fields.
        /// Returns the first detected violation according to validation order.)
        /// </summary>
        private sealed class RequiredFieldsRule : IValidationRule<DeviceInfoValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Presence;

            public ValidationFailure? Validate(DeviceInfoValidationData value)
            {
                return ValidateOperatingSystem(value.OperatingSystem)
                    ?? ValidateBrowser(value.Browser)
                    ?? ValidateDeviceModel(value.DeviceModel);
            }

            private static ValidationFailure? ValidateOperatingSystem(string? operatingSystem)
            {
                if (!string.IsNullOrWhiteSpace(operatingSystem))
                    return null;

                return CreateFailure(
                    nameof(DeviceInfo.OperatingSystem),
                    DeviceInfoErrors.EmptyOperatingSystem<DeviceInfo>()
                );
            }

            private static ValidationFailure? ValidateBrowser(string? browser)
            {
                if (!string.IsNullOrWhiteSpace(browser))
                    return null;

                return CreateFailure(
                    nameof(DeviceInfo.Browser),
                    DeviceInfoErrors.EmptyBrowser<DeviceInfo>()
                );
            }

            private static ValidationFailure? ValidateDeviceModel(string? deviceModel)
            {
                if (!string.IsNullOrWhiteSpace(deviceModel))
                    return null;

                return CreateFailure(
                    nameof(DeviceInfo.DeviceModel),
                    DeviceInfoErrors.EmptyDeviceModel<DeviceInfo>()
                );
            }

            private static ValidationFailure CreateFailure(
                string fieldName,
                Error error)
            {
                return new ValidationFailure()
                {
                    Error = error,
                    BuildException = operationType =>
                        DomainDataInconsistencyException.Empty<DeviceInfo>(
                            fieldName,
                            operationType
                        )
                };
            }
        }
    }
}
