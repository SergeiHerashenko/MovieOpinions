using Authorization.Application.Common.Enums;
using Authorization.Domain.Common.Errors;

namespace Authorization.Infrastructure.Errors.RateLimiter
{
    public static class RateLimiterErrors
    {
        public static Error LimitExceeded<TValue>(RateLimitAction action)
            => new(InfrastructureErrorCodes.LimitError.MaxLimit,
               $"Limit exceeded for {action}. Owner: {typeof(TValue).Name}!",
               ErrorType.Forbidden
            );
    }
}
