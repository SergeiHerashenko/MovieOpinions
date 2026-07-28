using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Rules
{
    public class EmptyDeviceModelRule : IValidationRule<DeviceInfoRuleValidationData, ValidationRestoreFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Presence;

        public ValidationRestoreFailure? Validate(DeviceInfoRuleValidationData value)
        {
            if (!string.IsNullOrWhiteSpace(value.DeviceModel))
                return null;

            return new ValidationRestoreFailure()
            {
                Error = RefreshTokenErrors.Device.EmptyDeviceModelName<DeviceInfo>(),
                BuildException = () => DomainDataInconsistencyException.Empty<DeviceInfo>(nameof(value))
            };
        }
    }
}
