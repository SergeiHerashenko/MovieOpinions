namespace Authorization.Domain.Common.Errors.TokenError
{
    public static class TokenError
    {
        public static Error Empty(string field)
            => new(ErrorCodes.TokenError.EmptyValue,
                   $"Failed to create token: field '{field}' cannot be empty!",
                   ErrorType.EmptyValue
            );

        public static Error InvalidFormat(string message)
            => new(ErrorCodes.TokenError.InvalidType,
                   message,
                   ErrorType.Validation
            );

        public static Error UsedToken(string message)
            => new(ErrorCodes.TokenError.TokenConsumed,
                   message,
                   ErrorType.Forbidden
            );

        public static Error ExpiredToken(string message)
            => new(ErrorCodes.TokenError.TokenExpired,
                   message,
                   ErrorType.Forbidden
            );

        public static Error RevokedToken(string message)
            => new(ErrorCodes.TokenError.TokenRevoked,
                   message,
                   ErrorType.Forbidden
            );

        public static Error TokenIsActive(string message)
            => new(ErrorCodes.TokenError.TokenActive,
                   message,
                   ErrorType.Forbidden
            );
    }
}
