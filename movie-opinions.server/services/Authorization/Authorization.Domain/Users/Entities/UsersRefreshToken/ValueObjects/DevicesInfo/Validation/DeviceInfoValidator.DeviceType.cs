using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Errors;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Validation
{
    internal static partial class DeviceInfoValidator
    {
        /// <summary>
        /// Перевіряє, що тип пристрою відповідає одному
        /// з визначених значень DeviceType.
        ///
        /// (Validates that the device type corresponds
        /// to one of the defined DeviceType values.)
        /// </summary>
        private sealed class ValidDeviceTypeRule : IValidationRule<DeviceInfoValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Format;

            public ValidationFailure? Validate(DeviceInfoValidationData data)
            {
                if (Enum.IsDefined(typeof(DeviceType), data.DeviceType))
                    return null;

                return new ValidationFailure()
                {
                    Error = DeviceInfoErrors.InvalidDeviceType<DeviceInfo>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.UnsupportedDiscriminator<DeviceInfo>(
                            nameof(DeviceInfo.DeviceType),
                            data.DeviceType,
                            operationType
                        )
                };
            }
        }
    }
}
