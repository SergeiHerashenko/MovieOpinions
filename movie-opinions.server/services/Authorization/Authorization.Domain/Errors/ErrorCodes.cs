namespace Authorization.Domain.Errors
{
    public  static class ErrorCodes
    {
        public static class LoginError
        {
            public const string Invalid = "LOGIN_INVALID";

            public const string InvalidEmail = "EMAIL_INVALID";

            public const string InvalidPhone = "PHONE_INVALID";

            public const string Empty = "LOGIN_EMPTY";
        }

        public static class PasswordError
        {
            public const string Empty = "PASSWORD_EMPTY";
        }

        public static class IdentifierError
        {
            public const string Empty = "IDENTIFIER_EMPTY";
        }

        public static class AccountStatusError
        {
            public const string Blocked = "USER_BLOCKED";

            public const string Deleted = "USER_DELETED";
        }

        public static class GeneralError
        {
            public const string OperationNotAllowed = "OPERATION_NOT_ALLOWED";
        }

        public static class UserPendingChangeError
        {
            public const string InvalidConfirmToken = "INVALID_CONFIRM_TOKEN";
        }

        public static class TokenError
        {
            public const string TokenEmpty = "ROKEN_EMPTY";

            public const string TokenInvalid = "TOKEN_INVALID";

            public const string TokenExpired = "TOKEN_EXPIRED";
        }

        public static class RestoreError
        {
            public const string NullReference = "NULL_REFERENCE";
        }
    }
}
