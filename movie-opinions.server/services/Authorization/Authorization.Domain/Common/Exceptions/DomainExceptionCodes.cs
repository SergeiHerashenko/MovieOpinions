namespace Authorization.Domain.Common.Exceptions
{
    internal static class DomainExceptionCodes
    {
        /// <summary>
        /// Діагностичні коди винятків неузгодженості доменних даних.
        ///
        /// (Diagnostic codes for domain data inconsistency exceptions.)
        /// </summary>
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

        /// <summary>
        /// Діагностичні коди винятків некоректного виконання доменних операцій.
        ///
        /// (Diagnostic codes for invalid domain operation exceptions.)
        /// </summary>
        public static class DomainInvalidOperation
        {
            public const string ValueAccessOnFailure =
                "DOMAIN.INVALID_OPERATION.VALUE_ACCESS_ON_FAILURE";

            public const string NullCallback =
                "DOMAIN.INVALID_OPERATION.NULL_CALLBACK";

            public const string PreconditionFailed =
                "DOMAIN.INVALID_OPERATION.PRECONDITION_FAILED";
        }

        /// <summary>
        /// Діагностичні коди винятків порушення доменних інваріантів.
        ///
        /// (Diagnostic codes for domain invariant violation exceptions.)
        /// </summary>
        public static class DomainInvariantViolation
        {
            public const string InvalidState =
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE";
        }
    }
}
