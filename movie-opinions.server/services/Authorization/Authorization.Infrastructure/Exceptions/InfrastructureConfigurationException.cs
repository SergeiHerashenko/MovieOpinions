using Authorization.Domain.Common.Errors;
using Authorization.Infrastructure.Errors;

namespace Authorization.Infrastructure.Exceptions
{
    public sealed class InfrastructureConfigurationException : BaseInfrastructureException
    {
        private InfrastructureConfigurationException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        public static InfrastructureConfigurationException ValueNotFound(
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            return new(
                InfrastructureErrorCodes.ConfigurationError.NotFoundValue,
                ErrorType.Conflict,
                message,
                context,
                innerException
            );
        }
    }
}
