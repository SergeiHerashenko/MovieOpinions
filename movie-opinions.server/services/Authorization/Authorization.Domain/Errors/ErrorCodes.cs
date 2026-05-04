namespace Authorization.Domain.Errors
{
    public  static class ErrorCodes
    {
        public static class LoginError
        {
            public const string Invalid = "LOGIN_INVALID";

            public const string Empty = "LOGIN_EMPTY";
        }

        public static class PasswordError
        {
            public const string Empty = "PASSWORD_EMPTY";

            public const string Short = "PASSWORD_SHORT";

            public const string Invalid = "PASSWORD_INVALID";
        }

        public static class RoleError
        {
            public const string Empty = "ROLE_EMPTY";
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
            public const string InvalidUserId = "USER_PENDING_CHANGE_INVALID_USER_ID";

            public const string InvalidConfirmToken = "INVALID_CONFIRM_TOKEN";
        }

        public static class TokenError
        {
            public const string TokenEmpty = "ROKEN_EMPTY";

            public const string TokenInvalid = "TOKEN_INVALID";

            public const string TokenExpired = "TOKEN_EXPIRED";
        }
    }
}
