namespace Authorization.Domain.Common.Errors.TokenError
{
    public static class IpError
    {
        public static Error InvalidFormat(string message)
            => new(ErrorCodes.IpError.InvalidFormat,
                   message,
                   ErrorType.Validation
            );
    }
}
