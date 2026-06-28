namespace Authorization.Application.Common.ApplicationErrors
{
    public static class ApplicationErrorCodes
    {
        public static class UserError
        {
            public const string AlreadyExists = "ALREADY_EXISTS";
        }

        public static class CommunicationError
        {
            public const string NotSent = "NOT_SENT";
        }
    }
}
