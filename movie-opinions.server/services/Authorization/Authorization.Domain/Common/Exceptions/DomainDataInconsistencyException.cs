using Authorization.Domain.Common.Errors;

namespace Authorization.Domain.Common.Exceptions
{
    public sealed class DomainDataInconsistencyException : BaseDomainException
    {
        private DomainDataInconsistencyException(string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        public static DomainDataInconsistencyException EmptyOnRestore(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                ErrorCodes.DataInconsistencyError.Inconsistency,
                ErrorType.BusinessRuleViolation,
                message,
                context,
                innerException
            );
        }

        public static DomainDataInconsistencyException UnsupportedDiscriminator(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                ErrorCodes.DataInconsistencyError.UnsupportedType,
                ErrorType.BusinessRuleViolation,
                message,
                context,
                innerException
            );
        }

        public static DomainDataInconsistencyException InvalidValue(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                ErrorCodes.DataInconsistencyError.InvalidState,
                ErrorType.BusinessRuleViolation,
                message,
                context,
                innerException
            );
        }
    }
}
