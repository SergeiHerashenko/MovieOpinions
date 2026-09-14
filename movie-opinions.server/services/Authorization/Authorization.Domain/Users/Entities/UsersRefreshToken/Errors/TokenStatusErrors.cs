using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.Errors
{
    /// <summary>
    /// Містить фабрики очікуваних помилок переходів
    /// між станами refresh-токена.
    ///
    /// (Provides factories for expected refresh-token
    /// lifecycle transition errors.)
    /// </summary>
    public static class TokenStatusErrors
    {
        public static Error ExpiredToken<TType>()
            => new(
                DomainErrorCodes.TokenStatus.ExpiredToken,
                $"Refresh token operation failed for type '{typeof(TType).Name}': " +
                "the token has expired and can no longer be used.",
                ErrorType.Forbidden
            );

        public static Error InvalidStatusTransition<TType>(
            TokenStatus currentStatus,
            TokenStatus targetStatus)
            => new(
                DomainErrorCodes.TokenStatus.InvalidStatusTransition,
                $"Refresh token status transition failed for type '{typeof(TType).Name}': " +
                $"transition from '{currentStatus}' to '{targetStatus}' is not allowed",
                ErrorType.Conflict
            );
    }
}
