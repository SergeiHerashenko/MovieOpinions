using Authorization.Domain.Users.Entities.UsersRefreshToken.Enums;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Validation
{
    internal static partial class DeviceInfoValidator
    {
        /// <summary>
        /// Містить сирі значення DeviceInfo, які передаються
        /// повному набору правил валідації.
        ///
        /// (Contains raw DeviceInfo values passed to the complete
        /// validation-rule set.)
        /// </summary>
        private sealed class DeviceInfoValidationData
        {
            public required DeviceType DeviceType { get; init; }

            public required string? OperatingSystem { get; init; }

            public required string? Browser { get; init; }

            public required string? DeviceModel { get; init; }
        }
    }
}
