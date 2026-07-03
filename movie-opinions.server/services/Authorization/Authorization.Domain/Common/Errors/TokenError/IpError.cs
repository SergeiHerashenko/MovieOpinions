namespace Authorization.Domain.Common.Errors.TokenError
{
    public static class IpError
    {
        public static Error InvalidFormat<TValue>(string value)
            => new(DomainErrorCodes.IpErrorCode.InvalidFormat,
                   $"Invalid IP address: {value}",
                   ErrorType.Validation
            );
    }
}
