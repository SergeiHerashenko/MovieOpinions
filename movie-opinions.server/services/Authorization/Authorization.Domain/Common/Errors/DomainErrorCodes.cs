namespace Authorization.Domain.Common.Errors
{
    /// <summary>
    /// Містить централізований каталог стабільних машинозчитуваних
    /// кодів очікуваних доменних помилок.
    ///
    /// Коди використовуються для ідентифікації, мапінгу та локалізації
    /// помилок незалежно від їхнього текстового повідомлення.
    ///
    /// (Provides a centralized catalog of stable machine-readable codes
    /// for expected domain errors.
    ///
    /// The codes are used for error identification, mapping, and localization
    /// independently of their human-readable messages.)
    /// </summary>
    /// <remarks>
    /// Після використання коду в API-контрактах, логах або клієнтських
    /// застосунках його значення слід вважати стабільним.
    ///
    /// (Once a code is used in API contracts, logs, or client applications,
    /// its value should be treated as stable.)
    /// </remarks>
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

        public static class EmailDomain
        {
            public const string EmptyDomainPart =
                "EMAIL_DOMAIN.EMPTY_DOMAIN_PART";

            public const string NotAllowedDomainPart =
                "EMAIL_DOMAIN.NOT_ALLOWED_DOMAIN_PART";

            public const string InvalidFormatEmailDomain =
                "EMAIL_DOMAIN.INVALID_FORMAT_EMAIL_DOMAIN";

            public const string TooLongEmailDomain =
                "EMAIL_DOMAIN.TOO_LONG_EMAIL_DOMAIN";

            public const string TooShortEmailDomain =
                "EMAIL_DOMAIN.TOO_SHORT_EMAIL_DOMAIN";
        }

        public static class EmailLocal
        {
            public const string EmptyEmailLocal =
                "EMAIL_LOCAL.EMPTY_EMAIL_LOCAL";

            public const string InvalidFormatEmailLocal =
                "EMAIL_LOCAL.INVALID_FORMAT_EMAIL_LOCAL";

            public const string TooLongEmailLocal =
                "EMAIL_LOCAL.TOO_LONG_EMAIL_LOCAL";

            public const string TooShortEmailLocal =
                "EMAIL_LOCAL.TOO_SHORT_EMAIL_LOCAL";
        }

        public static class Email
        {
            public const string EmptyEmail =
                "EMAIL.EMPTY_EMAIL";

            public const string InvalidFormatEmail =
                "EMAIL.INVALID_FORMAT_EMAIL";

            public const string TooLongEmail =
                "EMAIL.TOO_LONG_EMAIL";
        }

        public static class CountryCode
        {
            public const string EmptyCountryCode =
                "COUNTRY_CODE.EMPTY_COUNTRY_CODE";

            public const string InvalidFormatCountryCode =
                "COUNTRY_CODE.INVALID_FORMAT_COUNTRY_CODE";

            public const string TooLongCountryCode =
                "COUNTRY_CODE.TOO_LONG_COUNRY_CORE";

            public const string TooShortCountryCode =
                "COUNTRY_CODE.TOO_SHORT_COUNRY_CORE";
        }

        public static class NationalNumber
        {
            public const string EmptyNationalNumber =
                "NATIONAL_NUMBER.EMPTY_NATIONAL_NUMBER";

            public const string InvalidFormatNationalNumber =
                "NATIONAL_NUMBER.INVALID_FORMAT_NATIONAL_NUMBER";

            public const string TooLongNationalNumber =
                "NATIONAL_NUMBER.TOO_LONG_NATIONAL_NUMBER";

            public const string TooShortNationalNumber =
                "NATIONAL_NUMBER.TOO_SHORT_NATIONAL_NUMBER";
        }

        public static class Phone
        {
            public const string EmptyPhone =
                "PHONE.EMPTY_PHONE";

            public const string NotAllowedPhone =
                "PHONE.NOT_ALLOWED_PHONE";
        }

        public static class PlainPassword
        {
            public const string EmptyPlainPassword =
                "PLAIN_PASSWORD.EMPTY_PLAIN_PASSWORD";

            public const string MissingLowercaseLetterPlainPassword =
                "PLAIN_PASSWORD.MISSING_LOWERCASE_LETTER_PLAIN_PASSWORD";

            public const string MissingUppercaseLetterPlainPassword =
                "PLAIN_PASSWORD.MISSING_UPPERCASE_LETTER_PLAIN_PASSWORD";

            public const string MissingDigitPlainPassword =
                "PLAIN_PASSWORD.MISSING_DIGIT_PLAIN_PASSWORD";

            public const string TooLongPlainPassword =
                "PLAIN_PASSWORD.TOO_LONG_PLAIN_PASSWORD";

            public const string TooShortPlainPassword =
                "PLAIN_PASSWORD.TOO_SHORT_PLAIN_PASSWORD";
        }

        public static class AccessUser
        {
            public const string UserDeleted =
                "ACCESS_USER.USER_DELETED";

            public const string UserBlocked =
                "ACCESS_USER.USER_BLOCKED";
        }

        public static class RestrictionUser
        {
            public const string EmptyRestrictionList =
                "RESTRICTION_USER.EMPTY_RESTRICTION_LIST";

            public const string DuplicateRestrictionIdentifiers =
                "RESTRICTION_USER.DUPLICATED_RESTRICTION_IDENTIFIERS";

            public const string NotFoundSession =
                "RESTRICTION_USER.NOT_FOUND_SESSION";
        }
    }
}
