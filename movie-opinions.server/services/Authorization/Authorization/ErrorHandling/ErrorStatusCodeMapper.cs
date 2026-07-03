using Authorization.Domain.Common.Errors;

namespace Authorization.ErrorHandling
{
    public class ErrorStatusCodeMapper : IErrorStatusCodeMapper
    {
        private static readonly Dictionary<string, int> _map = new()
        {
            // Data consistency (500)
            [DomainErrorCodes.DataInconsistencyErrorCode.Inconsistency] = StatusCodes.Status500InternalServerError,
            [DomainErrorCodes.DataInconsistencyErrorCode.InvalidFormat] = StatusCodes.Status500InternalServerError,
            [DomainErrorCodes.DataInconsistencyErrorCode.UnsupportedType] = StatusCodes.Status500InternalServerError,
            [DomainErrorCodes.DataInconsistencyErrorCode.OutOfRange] = StatusCodes.Status500InternalServerError,

            // Invariants
            [DomainErrorCodes.InvariantViolationErrorCode.InvalidState] = StatusCodes.Status500InternalServerError,

            // Invalid operation
            [DomainErrorCodes.InvalidOperationErrorCode.InvalidOperation] = StatusCodes.Status500InternalServerError,

            // Email
            [DomainErrorCodes.EmailErrorCode.EmptyEmail] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.EmailErrorCode.InvalidFormatEmail] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.EmailErrorCode.TooLongEmail] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.EmailErrorCode.NotAllowedEmail] = StatusCodes.Status403Forbidden,

            // Phone
            [DomainErrorCodes.PhoneErrorCode.EmptyPhone] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.PhoneErrorCode.EmptyContryCode] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.PhoneErrorCode.InvalidFormatPhone] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.PhoneErrorCode.InvalidFormatCountryCode] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.PhoneErrorCode.TooLongPhone] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.PhoneErrorCode.TooShortPhone] = StatusCodes.Status400BadRequest,

            // Password
            [DomainErrorCodes.PasswordErrorCode.EmptyPassword] = StatusCodes.Status400BadRequest,

            // Login
            [DomainErrorCodes.LoginErrorCode.EmptyLogin] = StatusCodes.Status400BadRequest,

            // Access
            [DomainErrorCodes.AccessErrorCode.AccountBlocked] = StatusCodes.Status403Forbidden,
            [DomainErrorCodes.AccessErrorCode.AccountDeleted] =StatusCodes.Status403Forbidden,
            [DomainErrorCodes.AccessErrorCode.RestoreIsNotAllowed] =StatusCodes.Status403Forbidden,

            // General
            [DomainErrorCodes.GeneralErrorCode.OperationIsNotAllowed] = StatusCodes.Status403Forbidden,
            [DomainErrorCodes.GeneralErrorCode.NoChangesDetected] = StatusCodes.Status409Conflict,
            [DomainErrorCodes.GeneralErrorCode.AlreadyConfirmed] = StatusCodes.Status409Conflict,
            [DomainErrorCodes.GeneralErrorCode.AlreadyRestored] = StatusCodes.Status409Conflict,

            // Identifier
            [DomainErrorCodes.IdentifierErrorCode.EmptyIdentifier] = StatusCodes.Status400BadRequest,

            // Token
            [DomainErrorCodes.TokenErrorCode.EmptyToken] =StatusCodes.Status400BadRequest,
            [DomainErrorCodes.TokenErrorCode.InvalidType] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.TokenErrorCode.TokenConsumed] = StatusCodes.Status409Conflict,
            [DomainErrorCodes.TokenErrorCode.TokenExpired] = StatusCodes.Status401Unauthorized,
            [DomainErrorCodes.TokenErrorCode.TokenRevoked] = StatusCodes.Status401Unauthorized,
            [DomainErrorCodes.TokenErrorCode.TokenActive] = StatusCodes.Status409Conflict,

            // IP
            [DomainErrorCodes.IpErrorCode.InvalidFormat] = StatusCodes.Status400BadRequest,

            // Restriction rules
            [DomainErrorCodes.RestrictionRuleErrorCode.EmptyValue] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.RestrictionRuleErrorCode.ShortDay] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.RestrictionRuleErrorCode.InvalidTime] = StatusCodes.Status400BadRequest,
        };

        public int GetStatusCode(string errorCode)
        {
            if (_map.TryGetValue(errorCode, out var statusCode))
                return statusCode;

            return StatusCodes.Status500InternalServerError;
        }
    }
}
