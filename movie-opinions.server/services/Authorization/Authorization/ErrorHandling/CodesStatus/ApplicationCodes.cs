using Authorization.Application.Common.Errors;

namespace Authorization.ErrorHandling.CodesStatus
{
    internal static class ApplicationCodes
    {
        public static Dictionary<string, int> Values { get; } =
            new Dictionary<string, int>
            {
                // General errors
                [ApplicationErrorCodes.GeneralError.InvalidOperation] = StatusCodes.Status422UnprocessableEntity,

                // Users errors
                [ApplicationErrorCodes.UsersError.UserAlreadyExists] = StatusCodes.Status409Conflict,
                [ApplicationErrorCodes.UsersError.UserNotFound] = StatusCodes.Status404NotFound,
                [ApplicationErrorCodes.UsersError.UserIsBlocked] = StatusCodes.Status403Forbidden,
                [ApplicationErrorCodes.UsersError.UserIsPermanentlyDeleted] = StatusCodes.Status410Gone,
                [ApplicationErrorCodes.UsersError.UserIsDeleted] = StatusCodes.Status404NotFound,
                [ApplicationErrorCodes.UsersError.UserInvalidPassword] = StatusCodes.Status401Unauthorized,
                [ApplicationErrorCodes.UsersError.HasPendingAction] = StatusCodes.Status409Conflict,
                [ApplicationErrorCodes.UsersError.CredentialsChanged] = StatusCodes.Status401Unauthorized,

                // Confirm errors
                [ApplicationErrorCodes.ConfirmError.InvalidToken] = StatusCodes.Status400BadRequest,

                // Contact errors
                [ApplicationErrorCodes.ContactError.ContactInvariantViolated] = StatusCodes.Status422UnprocessableEntity,

                // Registration errors
                [ApplicationErrorCodes.RegistrationError.ConfirmPasswordRequired] = StatusCodes.Status400BadRequest,
                [ApplicationErrorCodes.RegistrationError.PasswordsDoNotMatch] = StatusCodes.Status400BadRequest,
                [ApplicationErrorCodes.RegistrationError.TermsMustBeAccepted] = StatusCodes.Status400BadRequest,
            };
    }
}
