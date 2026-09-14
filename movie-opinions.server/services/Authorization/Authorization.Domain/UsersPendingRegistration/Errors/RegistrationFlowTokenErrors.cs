using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.UsersPendingRegistration.Errors
{
    /// <summary>
    /// Містить очікувані помилки валідації RegistrationFlowToken.
    ///
    /// (Contains expected validation errors for RegistrationFlowToken.)
    /// </summary>
    public static class RegistrationFlowTokenErrors
    {
        public static Error Empty<TType>()
            => new(
                DomainErrorCodes.RegistrationFlowToken.Empty,
                $"Registration flow token validation failed for type '{typeof(TType).Name}': the value is empty!",
                ErrorType.Validation
            );

        public static Error InvalidLength<TType>(
            int actualLength,
            int expectedLength)
            => new(
                DomainErrorCodes.RegistrationFlowToken.InvalidLength,
                $"Registration flow token validation failed for type '{typeof(TType).Name}': " +
                $"expected encoded length is {expectedLength}, " +
                $"but the actual length is {actualLength}.",
                ErrorType.Validation
            );

        public static Error InvalidFormat<TType>()
            => new(
                DomainErrorCodes.RegistrationFlowToken.InvalidFormat,
                $"Registration flow token validation failed for type '{typeof(TType).Name}': " +
                $"the value is not valid Base64.",
                ErrorType.Validation
            );
    }
}
