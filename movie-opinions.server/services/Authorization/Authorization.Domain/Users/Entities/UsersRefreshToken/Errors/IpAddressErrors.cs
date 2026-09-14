using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.Errors
{
    /// <summary>
    /// Містить фабрики очікуваних помилок валідації IP-адреси.
    ///
    /// (Provides factories for expected IP-address validation errors.)
    /// </summary>
    public static class IpAddressErrors
    {
        public static Error Empty<TType>()
            => new(
                DomainErrorCodes.IpAddress.Empty,
                $"Ip address is empty or missing. Owner: {typeof(TType).Name}!",
                ErrorType.Validation
            );

        public static Error InvalidFormat<TType>()
            => new(
                DomainErrorCodes.IpAddress.InvalidFormat,
                $"Incorrect IP address format. Owner: {typeof(TType).Name}!",
                ErrorType.Validation
            );
    }
}
