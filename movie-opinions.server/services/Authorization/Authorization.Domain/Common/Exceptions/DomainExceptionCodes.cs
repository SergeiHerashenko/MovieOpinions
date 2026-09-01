namespace Authorization.Domain.Common.Exceptions
{
    /// <summary>
    /// Містить стабільні діагностичні коди внутрішніх винятків Domain.
    /// Коди призначені для логування, моніторингу та пошуку технічних проблем.
    /// Вони не повинні повертатися користувачеві або використовуватися для локалізації.
    ///
    /// (Contains stable diagnostic codes for internal domain exceptions.
    /// These codes are intended for logging, monitoring, and technical diagnostics.
    /// They must not be exposed to clients or used for localization.)
    /// </summary>
    internal static class DomainExceptionCodes
    {
        public static class DomainDataInconsistency
        {
            public const string EmptyValue =
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE";

            public const string InvalidFormat 
                = "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT";

            public const string UnsupportedType = 
                "DOMAIN.DATA_INCONSISTENCY.UNSUPPORTED_TYPE";

            public const string OutOfRange = 
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE";
        }

        public static class DomainInvalidOperation
        {
            public const string ValueAccessOnFailure =
                "DOMAIN.INVALID_OPERATION.VALUE_ACCESS_ON_FAILURE";

            public const string NullCallback =
                "DOMAIN.INVALID_OPERATION.NULL_CALLBACK";
        }

        public static class DomainInvariantViolation
        {
            public const string InvalidState = 
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE";
        }
    }
}
