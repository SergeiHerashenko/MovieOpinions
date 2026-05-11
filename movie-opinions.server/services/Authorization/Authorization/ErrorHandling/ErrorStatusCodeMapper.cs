using Authorization.Domain.Errors;

namespace Authorization.ErrorHandling
{
    public class ErrorStatusCodeMapper : IErrorStatusCodeMapper
    {
        private static readonly Dictionary<string, int> _map = new()
        {
            // Login validation
            [ErrorCodes.LoginError.Empty] = StatusCodes.Status400BadRequest,
            [ErrorCodes.LoginError.Invalid] = StatusCodes.Status400BadRequest,
            [ErrorCodes.LoginError.InvalidEmail] = StatusCodes.Status400BadRequest,
            [ErrorCodes.LoginError.InvalidPhone] = StatusCodes.Status400BadRequest,

            // Password validation
            [ErrorCodes.PasswordError.Empty] = StatusCodes.Status400BadRequest,

            // Generic identifier validation
            [ErrorCodes.IdentifierError.Empty] = StatusCodes.Status400BadRequest,

            // Account status
            [ErrorCodes.AccountStatusError.Blocked] = StatusCodes.Status403Forbidden,
            [ErrorCodes.AccountStatusError.Deleted] = StatusCodes.Status403Forbidden,

            // General business rule violation
            [ErrorCodes.GeneralError.OperationNotAllowed] = StatusCodes.Status403Forbidden,

            // Pending change token errors
            [ErrorCodes.UserPendingChangeError.InvalidConfirmToken] = StatusCodes.Status400BadRequest,

            // Token errors
            [ErrorCodes.TokenError.TokenEmpty] = StatusCodes.Status400BadRequest,
            [ErrorCodes.TokenError.TokenInvalid] = StatusCodes.Status401Unauthorized,
            [ErrorCodes.TokenError.TokenExpired] = StatusCodes.Status401Unauthorized,

            // Data restoration inconsistency (server error)
            [ErrorCodes.RestoreError.NullReference] = StatusCodes.Status500InternalServerError
        };

        public int GetStatusCode(string errorCode)
        {
            if (_map.TryGetValue(errorCode, out var status))
                return status;

            return StatusCodes.Status500InternalServerError;
        }
    }
}
