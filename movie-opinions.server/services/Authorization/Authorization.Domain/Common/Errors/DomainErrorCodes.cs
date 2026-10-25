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
        /// <summary>
        /// Коди помилок валідації телефонного коду країни.
        ///
        /// (Validation error codes for a telephone country code.)
        /// </summary>
        public static class CountryCode
        {
            public const string EmptyCountryCode =
                "COUNTRY_CODE.EMPTY_COUNTRY_CODE";

            public const string InvalidFormatCountryCode =
                "COUNTRY_CODE.INVALID_FORMAT_COUNTRY_CODE";

            public const string TooLongCountryCode =
                "COUNTRY_CODE.TOO_LONG_COUNTRY_CODE";

            public const string TooShortCountryCode =
                "COUNTRY_CODE.TOO_SHORT_COUNTRY_CODE";
        }

        /// <summary>
        /// Коди помилок валідації національної частини телефонного номера.
        ///
        /// (Validation error codes for the national part of a telephone number.)
        /// </summary>
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

        /// <summary>
        /// Коди помилок валідації телефонного номера.
        ///
        /// (Validation error codes for a telephone number.)
        /// </summary>
        public static class Phone
        {
            public const string EmptyPhone =
                "PHONE.EMPTY_PHONE";

            public const string NotAllowedPhone =
                "PHONE.NOT_ALLOWED_PHONE";

            public const string InvalidNumberingPlan =
                "PHONE.INVALID_NUMBERING_PLAN";
        }

        /// <summary>
        /// Коди помилок валідації доменної частини електронної адреси.
        ///
        /// (Validation error codes for the domain part of an email address.)
        /// </summary>
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

        /// <summary>
        /// Коди помилок валідації локальної частини електронної адреси.
        ///
        /// (Validation error codes for the local part of an email address.)
        /// </summary>
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

        /// <summary>
        /// Коди помилок валідації електронної адреси.
        ///
        /// (Validation error codes for an email address.)
        /// </summary>
        public static class Email
        {
            public const string EmptyEmail =
                "EMAIL.EMPTY_EMAIL";

            public const string InvalidFormatEmail =
                "EMAIL.INVALID_FORMAT_EMAIL";

            public const string TooLongEmail =
                "EMAIL.TOO_LONG_EMAIL";
        }

        /// <summary>
        /// Коди помилок валідації незахешованого пароля.
        ///
        /// (Validation error codes for a plain password.)
        /// </summary>
        public static class PlainPassword
        {
            public const string EmptyPlainPassword =
                "PLAIN_PASSWORD.EMPTY_PLAIN_PASSWORD";

            public const string MissingLowercaseLetterPlainPassword =
                "PLAIN_PASSWORD.MISSING_LOWERCASE_LETTER_PLAIN_PASSWORD";

            public const string MissingUppercaseLetterPlainPassword =
                "PLAIN_PASSWORD.MISSING_UPPERCASE_LETTER_PLAIN_PASSWORD";

            public const string MissingNumberPlainPassword =
                "PLAIN_PASSWORD.MISSING_NUMBER_PLAIN_PASSWORD";

            public const string TooLongPlainPassword =
                "PLAIN_PASSWORD.TOO_LONG_PLAIN_PASSWORD";

            public const string TooShortPlainPassword =
                "PLAIN_PASSWORD.TOO_SHORT_PLAIN_PASSWORD";
        }

        /// <summary>
        /// Коди помилок валідації токена процесу реєстрації.
        ///
        /// (Validation error codes for a registration flow token.)
        /// </summary>
        public static class RegistrationFlowToken
        {
            public const string Empty =
                "REGISTRATION_FLOW_TOKEN.EMPTY";

            public const string InvalidLength =
                "REGISTRATION_FLOW_TOKEN.INVALID_LENGTH";

            public const string InvalidFormat =
                "REGISTRATION_FLOW_TOKEN.INVALID_FORMAT";
        }

        /// <summary>
        /// Коди помилок видалення та відновлення користувача.
        ///
        /// (Error codes for user deletion and restoration.)
        /// </summary>
        public static class UserDeletion
        {
            public const string UserNotDeleted =
                "USER_DELETION.USER_NOT_DELETED";

            public const string TooLongReason =
                "USER_DELETION.TOO_LONG_REASON";

            public const string UserAlreadyRestored =
                "USER_DELETION.USER_ALREADY_RESTORED";

            public const string RestorationPeriodExpired =
                "USER_DELETION.RESTORATION_PERIOD_EXPIRED";

            public const string UserAlreadyDeleted =
                "USER_DELETION.USER_ALREADY_DELETED";
        }

        /// <summary>
        /// Коди помилок валідації токена процесу підтвердження.
        ///
        /// (Validation error codes for a confirmation flow token.)
        /// </summary>
        public static class ConfirmationFlowToken
        {
            public const string Empty =
                "CONFIRMATION_FLOW_TOKEN.EMPTY";

            public const string InvalidLength =
                "CONFIRMATION_FLOW_TOKEN.INVALID_LENGTH";

            public const string InvalidFormat =
                "CONFIRMATION_FLOW_TOKEN.INVALID_FORMAT";
        }

        /// <summary>
        /// Коди помилок операцій із відкладеними діями.
        ///
        /// (Error codes for operations on pending actions.)
        /// </summary>
        public static class PendingAction
        {
            public const string EmptyAction =
                "PENDING_ACTION.EMPTY_ACTION";

            public const string NotFoundAction =
                "PENDING_ACTION.NOT_FOUND_ACTION";

            public const string InvalidActionType =
                "PENDING_ACTION.INVALID_ACTION_TYPE";

            public const string ExpiredAction =
                "PENDING_ACTION.EXPIRED_ACTION";

            public const string InvalidConfirmationToken =
                "PENDING_ACTION.INVALID_CONFIRMATION_TOKEN";

            public const string NoUpdateNeeded =
                "PENDING_ACTION.NO_UPDATE_NEEDED";

            public const string ActionAlreadyExists =
                "PENDING_ACTION.ACTION_ALREADY_EXISTS";
        }

        /// <summary>
        /// Коди помилок пошуку токена, його строку дії та переходів між статусами.
        ///
        /// (Error codes for token lookup, expiration, and status transitions.)
        /// </summary>
        public static class TokenStatus
        {
            public const string NotFoundToken =
                "TOKEN_STATUS.NOT_FOUND_TOKEN";

            public const string ExpiredToken =
                "TOKEN_STATUS.EXPIRED_TOKEN";

            public const string InvalidStatusTransition =
                "TOKEN_STATUS.INVALID_STATUS_TRANSITION";
        }

        /// <summary>
        /// Коди помилок валідації інформації про пристрій.
        ///
        /// (Validation error codes for device information.)
        /// </summary>
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

        /// <summary>
        /// Коди помилок валідації IP-адреси.
        ///
        /// (Validation error codes for an IP address.)
        /// </summary>
        public static class IpAddress
        {
            public const string Empty =
                "IP_ADDRESS.EMPTY";

            public const string InvalidFormat =
                "IP_ADDRESS.INVALID_FORMAT";
        }

        /// <summary>
        /// Коди помилок валідації та зміни стану обмеження.
        ///
        /// (Error codes for restriction validation and state changes.)
        /// </summary>
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

        /// <summary>
        /// Коди помилок роботи з обмеженнями користувача та сесією обмежень.
        ///
        /// (Error codes for user restrictions and restriction sessions.)
        /// </summary>
        public static class RestrictionUser
        {
            public const string EmptyRestrictionList =
                "RESTRICTION_USER.EMPTY_RESTRICTION_LIST";

            public const string DuplicateRestrictionIdentifiers =
                "RESTRICTION_USER.DUPLICATED_RESTRICTION_IDENTIFIERS";

            public const string NotFoundSession =
                "RESTRICTION_USER.NOT_FOUND_SESSION";

            public const string NotFoundRestriction =
                "RESTRICTION_USER.NOT_FOUND_RESTRICTION";
        }

        /// <summary>
        /// Коди помилок доступу користувача відповідно до стану облікового запису.
        ///
        /// (Error codes for user access based on account state.)
        /// </summary>
        public static class AccessUser
        {
            public const string UserDeleted =
                "ACCESS_USER.USER_DELETED";

            public const string UserBlocked =
                "ACCESS_USER.USER_BLOCKED";
        }
    }
}
