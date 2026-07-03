using Authorization.Domain.UsersRefreshToken.Enums;

namespace Authorization.Domain.Common.Errors.TokenError
{
    public static class TokenError
    {
        public static Error Empty<TValue>(string fieldName)
            => new(DomainErrorCodes.TokenErrorCode.EmptyToken,
                   $"Failed to create token: field '{fieldName}' cannot be empty. Owner: {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );

        public static Error InvalidFormat<TValue>(string tokenStatus)
            => new(DomainErrorCodes.TokenErrorCode.InvalidType,
                   $"Unsupported token status: {tokenStatus}. Owner: {typeof(TValue).Name}!",
                   ErrorType.Validation
            );

        public static Error UsedToken<TValue>()
            => new(DomainErrorCodes.TokenErrorCode.TokenConsumed,
                   $"Token already used. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error ExpiredToken<TValue>()
            => new(DomainErrorCodes.TokenErrorCode.TokenExpired,
                   $"The token's lifetime has expired. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error RevokedToken<TValue>()
            => new(DomainErrorCodes.TokenErrorCode.TokenRevoked,
                   $"The token was manually revoked by the user or system. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error TokenIsActive<TValue>()
            => new(DomainErrorCodes.TokenErrorCode.TokenActive,
                   $"Token not yet expired. Owner {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );
    }
}
