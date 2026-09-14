using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Errors;

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

            public ValidationFailure? Validate(DeviceInfoValidationData value)
            {
                DomainGuard.AgainstUndefinedEnum<DeviceInfo>(
                    OperationType.Read,
                    (value.DeviceType, nameof(DeviceInfo.DeviceType)));

                return new ValidationFailure()
                {
                    Error = DeviceInfoErrors.InvalidDeviceType<DeviceInfo>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.UnsupportedDiscriminator<DeviceInfo>(
                            nameof(DeviceInfo.DeviceType),
                            value.DeviceType,
                            operationType
                        )
                };
            }
        }
    }
}
