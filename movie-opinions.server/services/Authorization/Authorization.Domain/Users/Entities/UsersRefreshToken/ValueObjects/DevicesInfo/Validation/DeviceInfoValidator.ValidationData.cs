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
            /// <summary>
            /// Тип пристрою, переданий на перевірку.
            ///
            /// (The device type supplied for validation.)
            /// </summary>
            public required DeviceType DeviceType { get; init; }

            /// <summary>
            /// Назва операційної системи, передана на перевірку.
            ///
            /// (The operating system name supplied for validation.)
            /// </summary>
            public required string OperatingSystem { get; init; }

            /// <summary>
            /// Назва браузера, передана на перевірку.
            ///
            /// (The browser name supplied for validation.)
            /// </summary>
            public required string Browser { get; init; }

            /// <summary>
            /// Назва або модель пристрою, передана на перевірку.
            ///
            /// (The device name or model supplied for validation.)
            /// </summary>
            public required string DeviceModel { get; init; }

            /// <inheritdoc/>
            public required OperationType OperationType { get; init; }
        }
    }
}
