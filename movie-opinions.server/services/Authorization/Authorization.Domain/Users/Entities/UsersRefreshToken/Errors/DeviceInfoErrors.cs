using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.Errors
{
    /// <summary>
    /// Містить очікувані помилки валідації інформації про пристрій.
    ///
    /// (Contains expected device-information validation errors.)
    /// </summary>
    public static class DeviceInfoErrors
    {
        public static Error EmptyOperatingSystem<TType>()
            => new(
                DomainErrorCodes.DeviceInfo.EmptyOperatingSystem,
                $"Device information validation failed for type " +
                $"'{typeof(TType).Name}': operating system is required.",
                ErrorType.Validation
            );

        public static Error EmptyBrowser<TType>()
            => new(
                DomainErrorCodes.DeviceInfo.EmptyBrowser,
                $"Device information validation failed for type " +
                $"'{typeof(TType).Name}': browser is required.",
                ErrorType.Validation
            );

        public static Error EmptyDeviceModel<TType>()
            => new(
                DomainErrorCodes.DeviceInfo.EmptyDeviceModel,
                $"Device information validation failed for type " +
                $"'{typeof(TType).Name}': device model is required.",
                ErrorType.Validation
            );

        public static Error InvalidDeviceType<TType>()
            => new(
                DomainErrorCodes.DeviceInfo.InvalidDeviceType,
                $"Device information validation failed for type " +
                $"'{typeof(TType).Name}': device type is invalid or unsupported.",
                ErrorType.Validation
            );

        public static Error TooLongOperatingSystem<TType>(
            int actualLength,
            int maximumLength)
            => new(
                DomainErrorCodes.DeviceInfo.TooLongOperatingSystem,
                $"Device information validation failed for type " +
                $"'{typeof(TType).Name}': operating system length is {actualLength}, " +
                $"but the maximum allowed is {maximumLength}.",
                ErrorType.Validation
            );

        public static Error TooLongBrowser<TType>(
            int actualLength,
            int maximumLength)
            => new(
                DomainErrorCodes.DeviceInfo.TooLongBrowser,
                $"Device information validation failed for type " +
                $"'{typeof(TType).Name}': browser length is {actualLength}, " +
                $"but the maximum allowed is {maximumLength}.",
                ErrorType.Validation
            );

        public static Error TooLongDeviceModel<TType>(
            int actualLength,
            int maximumLength)
            => new(
                DomainErrorCodes.DeviceInfo.TooLongDeviceModel,
                $"Device information validation failed for type " +
                $"'{typeof(TType).Name}': device model length is {actualLength}, " +
                $"but the maximum allowed is {maximumLength}.",
                ErrorType.Validation
            );
    }
}
