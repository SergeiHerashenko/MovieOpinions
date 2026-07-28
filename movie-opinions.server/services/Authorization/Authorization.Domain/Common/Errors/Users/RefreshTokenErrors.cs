using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Common.Errors.Users
{
    public static class RefreshTokenErrors
    {
        public static class Device
        {
            public static Error EmptyOperatingSystemName<TValue>()
                => new(DomainErrorCodes.RefreshToken.EmptyOperatingSystemName,
                       $"Operating system name missing or empty. Owner: {typeof(TValue).Name}!",
                       ErrorType.EmptyValue
                );

            public static Error EmptyBrowseName<TValue>()
                => new(DomainErrorCodes.RefreshToken.EmptyBrowseName,
                       $"Browser name missing or empty. Owner: {typeof(TValue).Name}!",
                       ErrorType.EmptyValue
                );

            public static Error EmptyDeviceModelName<TValue>()
                => new(DomainErrorCodes.RefreshToken.EmptyDeviceModelName,
                       $"Device model name missing or empty. Owner: {typeof(TValue).Name}!",
                       ErrorType.EmptyValue
                );

            public static Error EmptyDeviceInfo<TValue>()
                => new(DomainErrorCodes.RefreshToken.EmptyDeviceInfo,
                       $"Device info missing or empty. Owner: {typeof(TValue).Name}!",
                       ErrorType.EmptyValue
                );
        }

        public static class TokenStatus
        {
            public static Error NotFoundToken<TValue>()
                => new(DomainErrorCodes.RefreshToken.NotFoundToken,
                       $"Token not found. Owner: {typeof(TValue).Name}!",
                       ErrorType.NotFound
                );

            public static Error ExpiredToken<TValue>()
                => new(DomainErrorCodes.RefreshToken.ExpiredToken,
                       $"Refresh token is expired. Owner: {typeof(TValue).Name}!",
                       ErrorType.Forbidden
                );
        }
    }
}
