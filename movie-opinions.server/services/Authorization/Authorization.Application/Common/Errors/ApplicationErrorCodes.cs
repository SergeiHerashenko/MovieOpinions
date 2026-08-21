namespace Authorization.Application.Common.Errors
{
    public static class ApplicationErrorCodes
    {
        public static class GeneralError
        {
            public const string InvalidOperation = "INVALID_OPERATION";
        }

        public static class UsersError
        {
            public const string UserAlreadyExists = "USER_ALREADY_EXISTS";

            public const string UserNotFound = "USER_NOT_FOUND";

            public const string UserIsBlocked = "USER_IS_BLOCKED";

            public const string UserIsPermanentlyDeleted = "USER_IS_PERMANENTLY_DELETED";

            public const string UserIsDeleted = "USER_IS_DELETED";

            public const string UserInvalidPassword = "USER_INVALID_PASSWORD";

            public const string NoVerifiedContactChannels = "NO_CONTACT_CHANNELS";

            public const string HasPendingAction = "HAS_PENDING_ACTION";
        }

        public static class ConfirmError
        {
            public const string InvalidToken = "INVALID_TOKEN";
        }
    }
}
