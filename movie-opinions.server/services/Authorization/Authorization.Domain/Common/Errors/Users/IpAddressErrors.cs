using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Common.Errors.Users
{
    public static class IpAddressErrors
    {
        public static Error EmptyIpAddress<TValue>()
            => new(DomainErrorCodes.IpAddress.Empty,
                   $"Ip address is empty or missing. Owner: {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );

        public static Error InvalidFormatIpAddress<TValue>()
            => new(DomainErrorCodes.IpAddress.InvalidFormat,
                   $"Incorrect IP address format. Owner: {typeof(TValue).Name}!",
                   ErrorType.InvalidFormat
            );
    }
}
