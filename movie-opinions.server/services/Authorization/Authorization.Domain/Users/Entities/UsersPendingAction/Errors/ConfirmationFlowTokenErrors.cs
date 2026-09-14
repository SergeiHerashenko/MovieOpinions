using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.Errors
{
    /// <summary>
    /// Містить очікувані помилки валідації ConfirmationFlowToken.
    ///
    /// (Contains expected validation errors for ConfirmationFlowToken.)
    /// </summary>
    public static class ConfirmationFlowTokenErrors
    {
        public static Error Empty<TType>()
            => new(
                DomainErrorCodes.ConfirmationFlowToken.Empty,
                $"Confirmation flow token validation failed for type '{typeof(TType).Name}': the value is empty!",
                ErrorType.Validation
            );

        public static Error InvalidLength<TType>(
            int actualLength,
            int expectedLength)
            => new(
                DomainErrorCodes.ConfirmationFlowToken.InvalidLength,
                $"Confirmation flow token validation failed for type '{typeof(TType).Name}': " +
                $"expected encoded length is {expectedLength}, " +
                $"but the actual length is {actualLength}.",
                ErrorType.Validation
            );

        public static Error InvalidFormat<TType>()
            => new(
                DomainErrorCodes.ConfirmationFlowToken.InvalidFormat,
                $"Confirmation flow token validation failed for type '{typeof(TType).Name}': " +
                $"the value is not valid Base64.",
                ErrorType.Validation
            );
    }
}
