using Authorization.Application.Errors;
using Authorization.Domain.Errors;
using Authorization.Infrastructure.Errors;

namespace Authorization.ErrorHandling
{
    public class ErrorStatusCodeMapper : IErrorStatusCodeMapper
    {
        private static readonly Dictionary<string, int> _map = new()
        {
            // Login validation
            [DomainErrorCodes.LoginError.Empty] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.LoginError.Invalid] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.LoginError.InvalidEmail] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.LoginError.InvalidPhone] = StatusCodes.Status400BadRequest,

            // Password validation
            [DomainErrorCodes.PasswordError.Empty] = StatusCodes.Status400BadRequest,

            // Generic identifier validation
            [DomainErrorCodes.IdentifierError.Empty] = StatusCodes.Status400BadRequest,

            // Account status
            [DomainErrorCodes.AccountStatusError.Blocked] = StatusCodes.Status403Forbidden,
            [DomainErrorCodes.AccountStatusError.Deleted] = StatusCodes.Status403Forbidden,

            // General business rule violation
            [DomainErrorCodes.GeneralError.OperationNotAllowed] = StatusCodes.Status403Forbidden,

            // Pending change token errors
            [DomainErrorCodes.UserPendingChangeError.InvalidConfirmToken] = StatusCodes.Status400BadRequest,

            // Token errors
            [DomainErrorCodes.TokenError.TokenEmpty] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.TokenError.TokenInvalid] = StatusCodes.Status401Unauthorized,
            [DomainErrorCodes.TokenError.TokenExpired] = StatusCodes.Status401Unauthorized,

            // Data restoration inconsistency (server error)
            [DomainErrorCodes.RestoreError.NullReference] = StatusCodes.Status500InternalServerError,

            // Infrastructure Layer
            [InfrastructureErrorCodes.RateLimitError.TooManyAttempts] = StatusCodes.Status429TooManyRequests,
            [InfrastructureErrorCodes.RateLimitError.NotFoundConfiguration] = StatusCodes.Status500InternalServerError,

            // Application Layer
            [ApplicationErrorCodes.UnknowType.UnknownTypeStep] = StatusCodes.Status500InternalServerError,
            [ApplicationErrorCodes.ErrorUser.UserAlreadyExists] = StatusCodes.Status409Conflict,
        };

        public int GetStatusCode(string errorCode)
        {
            if (_map.TryGetValue(errorCode, out var status))
                return status;

            return StatusCodes.Status500InternalServerError;
        }
    }
}
