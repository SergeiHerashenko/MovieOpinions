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

        public static class UserDeletion
        {
            public const string TooLongReason =
                "USER_DELETION.TOO_LONG_REASON";

            public const string UserAlreadyRestored =
                "USER_DELETION.USER_ALREADY_RESTORED";

            public const string RestorationPeriodExpired =
                "USER_DELETION.RESTORATION_PERIOD_EXPIRED";
        }

        public static class ConfirmationFlowToken
        {
            public const string Empty =
                "CONFIRMATION_FLOW_TOKEN.EMPTY";

            public const string InvalidLength =
                "CONFIRMATION_FLOW_TOKEN.INVALID_LENGTH";

            public const string InvalidFormat =
                "CONFIRMATION_FLOW_TOKEN.INVALID_FORMAT";
        }

        public static class IpAddress
        {
            public const string Empty =
                "IP_ADDRESS.EMPTY";

            public const string InvalidFormat =
                "IP_ADDRESS.INVALID_FORMAT";
        }

        public static class DeviceInfo
        {
            public const string EmptyOperatingSystem =
                "DEVICE_INFO.EMPTY_OPERATING_SYSTEM";

            public const string EmptyBrowser =
                "DEVICE_INFO.EMPTY_BROWSER";

            public const string EmptyDeviceModel =
                "DEVICE_INFO.EMPTY_DEVICE_MODEL";

            public const string InvalidDeviceType =
                "DEVICE_INFO.INVALID_DEVICE_TYPE";

            public const string TooLongOperatingSystem =
                "DEVICE_INFO.TOO_LONG_OPERATING_SYSTEM";

            public const string TooLongBrowser =
                "DEVICE_INFO.TOO_LONG_BROWSER";

            public const string TooLongDeviceModel =
                "DEVICE_INFO.TOO_LONG_DEVICE_MODEL";
        }

        public static class PendingAction
        {
            public const string InvalidStatusTransition =
                "PENDING_ACTION.INVALID_STATUS_TRANSITION";

            public const string ExpiredAction =
                "PENDING_ACTION.EXPIRED_ACTION";

            public const string InvalidConfirmationToken =
                "PENDING_ACTION.INVALID_CONFIRMATION_TOKEN";
        }

        public static class TokenStatus
        {
            public const string ExpiredToken =
                "TOKEN_STATUS.EXPIRED_TOKEN";

            public const string InvalidStatusTransition =
                "TOKEN_STATUS.INVALID_STATUS_TRANSITION";
        }

        public static class Restriction
        {
            public const string EmptyNameRestriction =
                "RESTRICTION.EMPTY_NAME_RESTRICTION";

            public const string InvalidNumberMinutes =
                "RESTRICTION.INVALID_NUMBER_MINUTES";

            public const string AlreadyRevoked =
                "RESTRICTION.ALREADY_REVOKED";

            public const string AlreadyCompleted =
                "RESTRICTION.ALREADY_COMPLETED";
        }

        public static class RestrictionSession
        {
            public const string EmptyRestrictionList =
                "RESTRICTION_SESSION.EMPTY_RESTRICTION_LIST";
        }
    }
}
