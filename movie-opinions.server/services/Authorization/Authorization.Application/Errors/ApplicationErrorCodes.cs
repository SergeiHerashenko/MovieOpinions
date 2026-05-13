namespace Authorization.Application.Errors
{
    public static class ApplicationErrorCodes
    {
        public static class ErrorUser
        {
            public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
        }

        public static class UnknowType
        {
            public const string UnknownTypeStep = "UNKNOWN_TYPE_STEP";
        }
    }
}
