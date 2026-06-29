namespace Authorization.Infrastructure.Errors
{
    public static class InfrastructureErrorCodes
    {
        public static class ConfigurationError
        {
            public const string NotFoundValue = "NOT_FOUND_VALUE";
        }

        public static class LimitError
        {
            public const string MaxLimit = "MAX_LIMIT";
        }

        public static class DbError
        {
            public const string NoConnection = "NO_CONNECTION";

            public const string DataNoReceived = "DATA_NO_RECEIVED";

            public const string DataConsistency = "DATA_CONSISTENCY_ERROR";

            public const string NotFound = "NOT_FOUND";

            public const string MigrationError = "MIGRATION_ERROR";
        }
    }
}
