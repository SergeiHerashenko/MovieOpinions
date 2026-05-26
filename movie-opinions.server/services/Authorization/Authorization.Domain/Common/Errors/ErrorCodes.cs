namespace Authorization.Domain.Common.Errors
{
    public sealed class ErrorCodes
    {
        public static class ResultError
        {
            public const string InvariantViolation = "RESULT_INVARIANT_VIOLATION";

            public const string ValueAccessOnFailure = "RESULT_VALUE_ACCESS_ON_FAILURE";
        }

        public static class EmailError
        {
            public const string Empty = "EMAIL_EMPTY";

            public const string InvalidFormat = "EMAIL_INVALID_FORMAT";

            public const string TooLong = "EMAIL_TOO_LONG";

            public static string NotAllowed = "EMAIL_NOT_ALLOWED";
        }

        public static class PhoneError
        {
            public const string EmptyPhone = "PHONE_EMPTY";

            public const string EmptyCountryCode = "COUNTRY_CODE_EMPTY";

            public const string InvalidFormat = "PHONE_INVALID_FORMAT";

            public const string TooLong = "PHONE_TOO_LONG";

            public const string TooShort = "PHONE_TOO_SHORT";
        }

        public static class PasswordError
        {
            public const string Empty = "PASSWORD_EMPTY";
        }

        public static class DataInconsistencyError
        {
            public const string Inconsistency = "INCONSISTENCY_DATA";

            public const string UnsupportedType = "UNSUPPORTED_TYPE";
        }
    }
}
