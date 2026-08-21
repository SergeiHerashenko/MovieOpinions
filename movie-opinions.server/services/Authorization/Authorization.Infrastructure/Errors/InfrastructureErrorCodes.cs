namespace Authorization.Infrastructure.Errors
{
    public static class InfrastructureErrorCodes
    {
        public static class IntegrationError
        {
            public const string SendingError = "SENDING_ERROR";
        }

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
            public const string ConnectionStringNotFound = "CONNECTION_STRING_NOT_FOUND";

            public const string NoTransaction = "NO_TRANSACTION";

            public const string NotFound = "NOT_FOUND";

            public const string MigrationError = "MIGRATION_ERROR";

            public const string DatabaseError = "DATABASE_ERROR";

            public const string NestedTransaction = "NESTED_TRANSACTION";

            public const string DataConsistency = "DATA_CONSISTENCY_ERROR";

            public const string NotConsistentState = "NOT_CONSISTENT_STATE";
        }

        public static class UsereContextError
        {
            public const string InvalidUserContext = "AUTH.INVALID_USER_CONTEXT";
        }
    }
}
