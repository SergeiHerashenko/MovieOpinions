namespace Authorization.Application.Common.Errors
{
    public static class ApplicationErrorCodes
    {
        public static class GeneralError
        {
            public const string InvalidOperation = "APPLICATION.INVALID_OPERATION";
        }

        public static class UsersError
        {
            public const string UserAlreadyExists = "USER_ALREADY_EXISTS";

            public const string UserNotFound = "USER_NOT_FOUND";

            public const string UserIsBlocked = "USER_IS_BLOCKED";

            public const string UserIsPermanentlyDeleted = "USER_IS_PERMANENTLY_DELETED";

            public const string UserIsDeleted = "USER_IS_DELETED";

            public const string UserInvalidPassword = "USER_INVALID_PASSWORD";

            public const string HasPendingAction = "HAS_PENDING_ACTION";

            public const string CredentialsChanged = "CREDENTIALS_CHANGED";
        }

        public static class ConfirmError
        {
            public const string InvalidToken = "INVALID_TOKEN";
        }

        public static class ContactError
        {
            public const string ContactInvariantViolated = "CONTACT_INVARIANT_VIOLATED";
        }

        public static class RegistrationError
        {
            public const string ConfirmPasswordRequired = "CONFIRM_PASSWORD_REQUIRED";

            public const string PasswordsDoNotMatch = "PASSWORDS_DO_NOT_MATCH";

            public const string TermsMustBeAccepted = "TERMS_MUST_ACCEPTED";
        }
    }
}
