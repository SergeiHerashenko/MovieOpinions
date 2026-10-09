using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Enums;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Validation
{
    internal static partial class DeviceInfoValidator
    {
        /// <summary>
        /// Містить значення DeviceInfo та контекст доменної операції,
        /// що передаються повному набору правил валідації.
        ///
        /// (Contains DeviceInfo values and domain-operation context
        /// supplied to the complete validation-rule set.)
        /// </summary>
        private sealed class DeviceInfoValidationData : IHasOperationType
        {
            public required DeviceType DeviceType { get; init; }

            public required string OperatingSystem { get; init; }

            public required string Browser { get; init; }

            public required string DeviceModel { get; init; }

            public required OperationType OperationType { get; init; }
        }
    }
}
