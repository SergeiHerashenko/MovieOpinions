using Authorization.Domain.Common.Errors;

namespace Authorization.ErrorHandling
{
    public class ErrorStatusCodeMapper : IErrorStatusCodeMapper
    {
        private static readonly Dictionary<string, int> _map = new()
        {
            // General errors
            [DomainErrorCodes.General.InvalidState] = StatusCodes.Status409Conflict,
            [DomainErrorCodes.General.InvalidOperation] = StatusCodes.Status409Conflict,
            [DomainErrorCodes.General.UnsupportedType] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.General.NoUpdateNeeded] = StatusCodes.Status409Conflict,
            [DomainErrorCodes.General.Expired] = StatusCodes.Status410Gone,

            // Identifier
            [DomainErrorCodes.Identifier.Empty] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Identifier.IdentifierMismatch] = StatusCodes.Status409Conflict,

            // Data
            [DomainErrorCodes.Data.EmptyValue] = StatusCodes.Status500InternalServerError,
            [DomainErrorCodes.Data.InvalidFormat] = StatusCodes.Status500InternalServerError,
            [DomainErrorCodes.Data.UnsupportedType] = StatusCodes.Status500InternalServerError,
            [DomainErrorCodes.Data.OutOfRange] = StatusCodes.Status500InternalServerError,

            // Email
            [DomainErrorCodes.Email.EmptyEmailDomain] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Email.NotAllowedEmailDomain] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Email.InvalidFormatEmailDomainPart] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Email.TooLongEmailDomainPart] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Email.TooShortEmailDomainPart] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Email.EmptyEmailLocalPart] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Email.InvalidFormatEmailLocalPart] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Email.TooLongEmailLocalPart] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Email.TooShortEmailLocalPart] = StatusCodes.Status400BadRequest,

            // Phone
            [DomainErrorCodes.Phone.EmptyPhoneCountryCode] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Phone.InvalidFormatPhoneCountryCode] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Phone.TooLongPhoneCountryCode] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Phone.TooShortPhoneCountryCode] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Phone.EmptyPhoneNationalNumber] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Phone.InvalidFormatPhoneNationalNumber] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Phone.TooLongPhoneNationalNumber] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Phone.TooShortPhoneNationalNumber] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Phone.NotAllowedPhone] = StatusCodes.Status400BadRequest,

            // Password
            [DomainErrorCodes.Password.EmptyPlainPassword] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Password.MissingLowercaseLetterPlainPassword] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Password.MissingUppercaseLetterPlainPassword] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Password.NoContainNumber] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Password.TooLongPlainPassword] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Password.TooShortPlainPassword] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Password.EmptyHashPassword] = StatusCodes.Status400BadRequest,

            // IpAddress
            [DomainErrorCodes.IpAddress.Empty] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.IpAddress.InvalidFormat] = StatusCodes.Status400BadRequest,

            // Login
            [DomainErrorCodes.Login.Empty] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Login.LoginIsNotConfirm] = StatusCodes.Status403Forbidden,

            // Restriction
            [DomainErrorCodes.Restriction.EmptyRestrictionList] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Restriction.EmptyRestrictionName] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Restriction.InvalidRestrictionType] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Restriction.EmptyRestriction] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Restriction.NotFoundRestriction] = StatusCodes.Status404NotFound,
            [DomainErrorCodes.Restriction.InvalidNumberMinutes] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Restriction.WrongTime] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Restriction.EmptyRestrictionRule] = StatusCodes.Status400BadRequest,

            // Restriction srssion
            [DomainErrorCodes.RestrictionSession.NotFoundSession] = StatusCodes.Status404NotFound,
            [DomainErrorCodes.RestrictionSession.NotFoundSessionType] = StatusCodes.Status404NotFound,

            // Deletion
            [DomainErrorCodes.Deletion.NotDeleteUser] = StatusCodes.Status409Conflict,

            // Access
            [DomainErrorCodes.Access.UserIsBlocked] = StatusCodes.Status403Forbidden,
            [DomainErrorCodes.Access.UserIsDeleted] =StatusCodes.Status403Forbidden,

            // RefreshToken
            [DomainErrorCodes.RefreshToken.EmptyDeviceInfo] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.RefreshToken.EmptyOperatingSystemName] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.RefreshToken.EmptyBrowseName] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.RefreshToken.EmptyDeviceModelName] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.RefreshToken.ExpiredToken] = StatusCodes.Status401Unauthorized,
            [DomainErrorCodes.RefreshToken.NotFoundToken] = StatusCodes.Status401Unauthorized,

            // Change
            [DomainErrorCodes.Change.EmptyChange] = StatusCodes.Status400BadRequest,
            [DomainErrorCodes.Change.InvalidConfirmationToken] = StatusCodes.Status401Unauthorized,
            [DomainErrorCodes.Change.ChangeAlreadyExists] = StatusCodes.Status409Conflict,
            [DomainErrorCodes.Change.InvalidChangeType] = StatusCodes.Status400BadRequest,
        };

        public int GetStatusCode(string errorCode)
        {
            if (_map.TryGetValue(errorCode, out var statusCode))
                return statusCode;

            return StatusCodes.Status500InternalServerError;
        }
    }
}
