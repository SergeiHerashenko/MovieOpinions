using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.Errors
{
    /// <summary>
    /// Містить очікувані доменні помилки, пов’язані
    /// з підтвердженням і життєвим циклом відкладених дій.
    ///
    /// (Contains expected domain errors related to confirmation
    /// and the lifecycle of pending actions.)
    /// </summary>
    public static class PendingActionErrors
    {
        public static Error InvalidStatusTransition<TType>(
            ActionStatus currentStatus,
            ActionStatus targetStatus)
            => new(
                DomainErrorCodes.PendingAction.InvalidStatusTransition,
                $"Cannot transition '{typeof(TType).Name}' from status '{currentStatus}' to '{targetStatus}'!",
                ErrorType.Conflict
            );

        public static Error ExpiredAction<TType>()
            => new(
                DomainErrorCodes.PendingAction.ExpiredAction,
                $"Pending action processing failed for type '{typeof(TType).Name}': " +
                "the action has expired.",
                ErrorType.Conflict
            );

        public static Error InvalidConfirmationToken<TType>()
            => new(
                DomainErrorCodes.PendingAction.InvalidConfirmationToken,
                $"Pending action confirmation failed for type '{typeof(TType).Name}': " +
                "the provided confirmation token does not match.",
                ErrorType.Forbidden
            );
    }
}
