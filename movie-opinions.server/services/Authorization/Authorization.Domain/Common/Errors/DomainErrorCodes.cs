namespace Authorization.Domain.Common.Errors
{
    public static class DomainErrorCodes
    {
        public static class RegistrationFlowToken
        {
            public const string Empty =
                "REGISTRATION_FLOW_TOKEN.EMPTY";

            public const string InvalidLength =
                "REGISTRATION_FLOW_TOKEN.INVALID_LENGTH";

            public const string InvalidFormat =
                "REGISTRATION_FLOW_TOKEN.INVALID_FORMAT";
        }
























































        public static class DomainExceptions
        {
            public const string UnsupportedType = "UNSUPPORTED_TYPE";
        }

        // Загальні помилки (General errors)
        public static class General
        {
            public const string NoUpdateNeeded = "NO_UPDATE_NEEDED";

            public const string Expired = "EXPIRED";

            public const string ActionCancelled = "ACTION_CANCELLED";
        }

        // Ідентифікатори
        public static class Identifier
        {
            public const string Empty = "EMPTY_IDENTIFIER";

            public const string IdentifierMismatch = "IDENTIFIER_MISMATCH";
        }


        // Email
        public static class Email
        {
            public const string EmptyEmail = "EMPTY_EMAIL";

            public const string InvalidFormatEmail = "INVALID_FORMAT_EMAIL";

            public const string EmptyEmailDomain = "EMAIL_DOMAIN_EMPTY";

            public const string NotAllowedEmailDomain = "NOT_ALLOWED_EMAIL_DOMAIN";

            public const string InvalidFormatEmailDomainPart = "INVALID_FORMAT_EMAIL_DOMAIN_PART";

            public const string TooLongEmailDomainPart = "TOO_LONG_EMAIL_DOMAIN_PART";

            public const string TooShortEmailDomainPart = "TOO_SHORT_EMAIL_DOMAIN_PART";

            public const string EmptyEmailLocalPart = "EMAIL_LOCAL_PART_EMPTY";

            public const string InvalidFormatEmailLocalPart = "INVALID_FORMAT_EMAIL_LOCAL_PART";

            public const string TooLongEmailLocalPart = "TOO_LONG_EMAIL_LOCAL_PART";

            public const string TooShortEmailLocalPart = "TOO_SHORT_EMAIL_LOCAL_PART";
        }

        // Phone
        public static class Phone
        {
            public const string EmptyPhoneCountryCode = "PHONE_COUNTRY_CODE_EMPTY";

            public const string InvalidFormatPhoneCountryCode = "INVALID_FORMAT_PHONE_COUNTRY_CODE";

            public const string TooLongPhoneCountryCode = "TOO_LONG_PHONE_COUNTRY_CODE";

            public const string TooShortPhoneCountryCode = "TOO_SHORT_PHONE_COUNTRY_CODE";

            public const string EmptyPhoneNationalNumber = "PHONE_NATIONAL_NUMBER_EMPTY";

            public const string InvalidFormatPhoneNationalNumber = "INVALID_FORMAT_PHONE_NATIONAL_NUMBER";

            public const string TooLongPhoneNationalNumber = "TOO_LOGN_PHONE_NATIONAL_NUMBER";

            public const string TooShortPhoneNationalNumber = "TOO_SHORT_PHONE_NATIONAL_NUMBER";

            public const string NotAllowedPhone = "NOT_ALLOWED_PHONE";
        }

        // Password 
        public static class Password
        {
            public const string EmptyPlainPassword = "EMPTY_PASSWORD";

            public const string MissingLowercaseLetterPlainPassword = "MISSING_LOWERCASE_LETTER_PASSWORD";

            public const string MissingUppercaseLetterPlainPassword = "MISSING_UPPERCASE_LETTER_PASSWORD";

            public const string NoContainNumber = "NO_CONTAIN_NUMBER";

            public const string TooLongPlainPassword = "TOO_LONG_PASSWORD";

            public const string TooShortPlainPassword = "TOO_SHORT_PASSWORD";

            public const string EmptyHashPassword = "EMPTY_HASH_PASSWORD";
        }

        // IpAddress
        public static class IpAddress
        {
            public const string Empty = "EMPTY_IP_ADDRESS";

            public const string InvalidFormat = "INVALID_FORMAT_IP_ADDRESS";
        }

        // Login
        public static class Login
        {
            public const string Empty = "LOGIN_EMPTY";

            public const string LoginIsNotConfirm = "LOGIN_IS_NOT_CONFIRM";
        }

        // Restriction
        public static class Restriction
        {
            public const string EmptyRestrictionList = "EMPTY_RESTRICTION_LIST";

            public const string EmptyRestrictionName = "EMPTY_RESTRICTION_NAME";

            public const string InvalidRestrictionType = "INVALID_RESTRICTION_TYPE";

            public const string EmptyRestriction = "EMPTY_RESTRICTION";

            public const string NotFoundRestriction = "NOT_FOUND_RESTRICTION";

            public const string InvalidNumberMinutes = "INVALID_NUMBER_MINUTES";

            public const string WrongTime = "WRONG_TIME";

            public const string EmptyRestrictionRule = "EMPTY_RESTRICTION_RULE";
        }

        // RestrictionSession
        public static class RestrictionSession
        {
            public const string NotFoundSession = "NOT_FOUND_SESSION";

            public const string NotFoundSessionType = "NOT_FOUND_SESSION_TYPE";
        }

        // Deletion
        public static class Deletion
        {
            public const string NotDeleteUser = "NO_DELETE_USER";

            public const string TooLongReason = "TOO_LONG_REASON";

            public const string NotFoundAction = "NO_FOUND_ACTION";
        }

        // Access
        public static class Access
        {
            public const string UserIsBlocked = "USER_BLOCKED";

            public const string UserIsDeleted = "USER_DELETED";
        }

        // RefreshToken
        public static class RefreshToken
        {
            public const string EmptyDeviceInfo = "EMPTY_DEVICE_INFO";

            public const string EmptyOperatingSystemName = "EMPTY_OPERATION_SYSTEM_NAME";

            public const string EmptyBrowseName = "EMPTY_BROWSE_NAME";

            public const string EmptyDeviceModelName = "EMPTY_DEVICE_MODEL_NAME";

            public const string ExpiredToken = "EXPIRED_TOKEN";

            public const string NotFoundToken = "NOT_FOUND_TOKEN";

            public const string TokenIsNotActive = "TOKEN_IS_NOT_ACTIVE";
        }

        // Action
        public static class Action
        {
            public const string EmptyAction = "EMPTY_ACTION";

            public const string InvalidConfirmationToken = "INVALID_CONFIRMATION_TOKEN";

            public const string ActionAlreadyExists = "ACTION_ALREADY_EXISTS";

            public const string InvalidActionType = "INVALID_ACTION_TYPE";

            public const string InvalidUserPendingActionId = "INVALID_USER_PENDING_ACTION_ID";

            public const string InvalidStatusTransition = "INVALID_STATUS_TRANSITION";

            public const string InvalidInputData = "INVALID_INPUT_DATA";
        }
    }
}
