namespace Authorization.Domain.Common.Errors
{
    public sealed class ErrorCodes
    {
        public static class DataInconsistencyError
        {
            public const string Inconsistency = "INCONSISTENCY_DATA";

            public const string UnsupportedType = "UNSUPPORTED_TYPE";

            public const string InvalidState = "INVALID_STATE";

            public const string InvalidValue = "INVALID_VALUE";
        }

        public static class ResultError
        {
            public const string InvariantViolation = "RESULT_INVARIANT_VIOLATION";

            public const string ValueAccessOnFailure = "RESULT_VALUE_ACCESS_ON_FAILURE";
        }

        public static class EmailError
        {
            public const string EmptyEmail = "EMAIL_EMPTY";

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
            public const string EmptyPassword = "PASSWORD_EMPTY";
        }

        public static class UserError
        {
            public static string Empty = "EMPTY_CONTENT_IN_ENTITY";

            public static string OperationIsNotAllowed = "OPERATION_IS_NOT_ALLOWED";
        }

        public static class AccountStatusError
        {
            public static string AccountBlocked = "ACCOUNT_BLOCKED";

            public static string AccountDeleted = "ACCOUNT_DELETED";

            public static string AccountIsNotRestore = "ACCOUNT_IS_NOT_RESTORE";
        }

        public static class ChangeError
        {
            public static string UpdateNotRequired = "UPDATE_NOT_REQUIRED";
        }

        public static class IpError
        {
            public static string InvalidFormat = "INVALID_FORMAT_IP";
        }

        public static class TokenError
        {
            public static string EmptyValue = "EMPTY_VALUE";

            public static string TokenActive = "TOKEN_IS_ACTIVE";

            public static string TokenConsumed = "TOKEN_CONSUMED";

            public static string TokenExpired = "TOKEN_EXPIRED";

            public static string TokenRevoked = "TOKEN_REVOKED";

            public static string InvalidType = "INVALID_TYPE";
        }

        public static class RestrictionError
        {
            public static string EmptyValue = "EMPTY_VALUE";

            public static string ShortDay = "SHORT_DAY";

            public static string InvalidTime = "INVALID_TIME";
        }
    }
}
