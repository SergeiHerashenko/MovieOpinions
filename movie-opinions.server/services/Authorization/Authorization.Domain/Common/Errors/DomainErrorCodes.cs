namespace Authorization.Domain.Common.Errors
{
    public sealed class DomainErrorCodes
    {
        public static class DataInconsistencyErrorCode
        {
            public const string Inconsistency = "INCONSISTENT_DATA";

            public const string InvalidFormat = "INVALID_FORMAT";

            public const string UnsupportedType = "UNSUPPORTED_TYPE";

            public const string OutOfRange = "PUT_OF_RANGE";
        }

        public static class InvariantViolationErrorCode
        {
            public const string InvalidState = "INVALID_STATE";
        }

        public static class InvalidOperationErrorCode
        {
            public const string InvalidOperation = "INVALID_OPERATION";
        }

        public static class EmailErrorCode
        {
            public const string EmptyEmail = "EMPTY_EMAIL";

            public const string InvalidFormatEmail = "INVALID_FORMAT_EMAIL";

            public const string TooLongEmail = "TOO_LONG_EMAIL";

            public const string NotAllowedEmail = "NOT_ALLOWED_EMAIL";
        }

        public static class PhoneErrorCode
        {
            public const string EmptyPhone = "EMPTY_PHONE";

            public const string EmptyContryCode = "EMPTY_COUNTRY_CODE";

            public const string InvalidFormatPhone = "INVALID_FORMAT_PHONE";

            public const string InvalidFormatCountryCode = "INVALID_FORMAT_COUNTRY_CODE";

            public const string TooLongPhone = "TOO_LONG_PHONE";

            public const string TooShortPhone = "TOO_SHORT_PHONE";
        }

        public static class PasswordErrorCode
        {
            public const string EmptyPassword = "PASSWORD_EMPTY";
        }

        public static class LoginErrorCode
        {
            public const string EmptyLogin = "LOGIN_EMPTY";
        }

        public static class AccessErrorCode
        {
            public const string AccountBlocked = "ACCOUNT_BLOCKED";

            public const string AccountDeleted = "ACCOUNT_DELETED";

            public const string RestoreIsNotAllowed = "RESTORE_IS_NOT_ALLOWED";
        }

        public static class GeneralErrorCode
        {
            public const string OperationIsNotAllowed = "OPERATION_IS_NOT_ALLOWED";

            public const string NoChangesDetected = "NO_CHANGES_DETECTED";

            public const string AlreadyConfirmed = "ALREADY_CONFIRMED";

            public const string AlreadyRestored = "ALREADY_RESTORED";
        }

        public static class IdentifierErrorCode
        {
            public const string EmptyIdentifier = "EMPTY_IDENTIFIER";
        }

        public static class TokenErrorCode
        {
            public const string EmptyToken = "TOKEN_EMPTY";

            public static string InvalidType = "INVALID_TYPE";

            public static string TokenConsumed = "TOKEN_CONSUMED";

            public static string TokenExpired = "TOKEN_EXPIRED";

            public static string TokenRevoked = "TOKEN_REVOKED";

            public static string TokenActive = "TOKEN_ACTIVE";
        }

        public static class IpErrorCode
        {
            public const string InvalidFormat = "INVALID_FORMAT_IP";
        }

        public static class RestrictionRuleErrorCode
        {
            public const string EmptyValue = "EMPTY_VALUE";

            public const string ShortDay = "NOT_ENOUGH_DAYS";

            public const string InvalidTime = "INVALID_TIME";
        }
    }
}