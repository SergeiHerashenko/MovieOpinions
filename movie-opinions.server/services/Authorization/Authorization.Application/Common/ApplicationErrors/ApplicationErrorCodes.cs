namespace Authorization.Application.Common.ApplicationErrors
{
    public static class ApplicationErrorCodes
    {
        public static class UserErrorCode
        {
            public const string AlreadyExists = "ALREADY_EXISTS";
        }

        public static class CommunicationErrorCode
        {
            public const string NotSent = "NOT_SENT";
        }

        public static class InvalidOperationErrorCode
        {
            public const string InvalidOperation = "INVALID_OPERATION";
        }

        public static class ConfirmErrorCode
        {
            public const string InvalidToken = "INVALID_TOKEN";
        }
    }
}
