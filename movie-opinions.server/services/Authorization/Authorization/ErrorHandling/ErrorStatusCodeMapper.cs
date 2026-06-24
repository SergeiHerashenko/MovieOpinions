using Authorization.Domain.Common.Errors;

namespace Authorization.ErrorHandling
{
    public class ErrorStatusCodeMapper : IErrorStatusCodeMapper
    {
        private static readonly Dictionary<string, int> _map = new()
        {
            [ErrorCodes.DataInconsistencyError.Inconsistency] = StatusCodes.Status500InternalServerError,
            [ErrorCodes.DataInconsistencyError.UnsupportedType] = StatusCodes.Status400BadRequest,
            [ErrorCodes.DataInconsistencyError.InvalidState] = StatusCodes.Status409Conflict,
            [ErrorCodes.DataInconsistencyError.InvalidValue] = StatusCodes.Status400BadRequest,

            [ErrorCodes.ResultError.InvariantViolation] = StatusCodes.Status500InternalServerError,
            [ErrorCodes.ResultError.ValueAccessOnFailure] = StatusCodes.Status500InternalServerError,

            [ErrorCodes.EmailError.EmptyEmail] = StatusCodes.Status400BadRequest,
            [ErrorCodes.EmailError.InvalidFormat] = StatusCodes.Status400BadRequest,
            [ErrorCodes.EmailError.TooLong] = StatusCodes.Status400BadRequest,
            [ErrorCodes.EmailError.NotAllowed] = StatusCodes.Status403Forbidden,

            [ErrorCodes.PhoneError.EmptyPhone] = StatusCodes.Status400BadRequest,
            [ErrorCodes.PhoneError.EmptyCountryCode] = StatusCodes.Status400BadRequest,
            [ErrorCodes.PhoneError.InvalidFormat] = StatusCodes.Status400BadRequest,
            [ErrorCodes.PhoneError.TooLong] = StatusCodes.Status400BadRequest,
            [ErrorCodes.PhoneError.TooShort] = StatusCodes.Status400BadRequest,

            [ErrorCodes.PasswordError.EmptyPassword] = StatusCodes.Status400BadRequest,

            [ErrorCodes.UserError.Empty] = StatusCodes.Status400BadRequest,
            [ErrorCodes.UserError.OperationIsNotAllowed] = StatusCodes.Status403Forbidden,

            [ErrorCodes.AccountStatusError.AccountBlocked] = StatusCodes.Status403Forbidden,
            [ErrorCodes.AccountStatusError.AccountDeleted] = StatusCodes.Status403Forbidden,
            [ErrorCodes.AccountStatusError.AccountIsNotRestore] = StatusCodes.Status403Forbidden,

            [ErrorCodes.ChangeError.UpdateNotRequired] = StatusCodes.Status409Conflict,

            [ErrorCodes.IpError.InvalidFormat] = StatusCodes.Status400BadRequest,

            [ErrorCodes.TokenError.EmptyValue] = StatusCodes.Status400BadRequest,
            [ErrorCodes.TokenError.TokenActive] = StatusCodes.Status409Conflict,
            [ErrorCodes.TokenError.TokenConsumed] = StatusCodes.Status409Conflict,
            [ErrorCodes.TokenError.TokenExpired] = StatusCodes.Status401Unauthorized,
            [ErrorCodes.TokenError.TokenRevoked] = StatusCodes.Status401Unauthorized,
            [ErrorCodes.TokenError.InvalidType] = StatusCodes.Status400BadRequest,

            [ErrorCodes.RestrictionError.EmptyValue] = StatusCodes.Status400BadRequest,
            [ErrorCodes.RestrictionError.ShortDay] = StatusCodes.Status400BadRequest,
            [ErrorCodes.RestrictionError.InvalidTime] = StatusCodes.Status400BadRequest,
        };

        public int GetStatusCode(string errorCode)
        {
            if (_map.TryGetValue(errorCode, out var statusCode))
                return statusCode;

            return StatusCodes.Status500InternalServerError;
        }
    }
}
