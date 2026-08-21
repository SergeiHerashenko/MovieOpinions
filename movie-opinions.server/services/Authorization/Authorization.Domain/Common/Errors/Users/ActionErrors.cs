using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

namespace Authorization.Domain.Common.Errors.Users
{
    public static class ActionErrors
    {
        public static Error EmptyAction<TValue>()
            => new(DomainErrorCodes.Action.EmptyAction,
                   $"Action is missing or empty. Owner: {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );

        public static Error Expired<TValue>()
            => new(DomainErrorCodes.General.Expired,
                   $"Action already expired. Owner: {typeof(TValue).Name}!",
                   ErrorType.InvalidOperation
            );

        public static Error InvalidConfirmationToken<TValue>()
            => new(DomainErrorCodes.Action.InvalidConfirmationToken,
                   $"Confirmation token invslid. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error InvalidUserPendingActionId<TValue>()
            => new(DomainErrorCodes.Action.InvalidUserPendingActionId,
                   $"User Pending Action Id invalid. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error ActionAlreadyExists<TValue>()
            => new(DomainErrorCodes.Action.ActionAlreadyExists,
                   $"The action already exists. Owner: {typeof(TValue).Name}!",
                   ErrorType.Conflict
            );

        public static Error InvalidActionType<TValue>()
            => new(DomainErrorCodes.Action.InvalidActionType,
                   $"Invalid action type. Owner: {typeof(TValue).Name}!",
                   ErrorType.Conflict
            );

        public static Error InvalidStatusTransition<TValue>(ActionStatus currentStatus, ActionStatus expectedStatus)
            => new(DomainErrorCodes.Action.InvalidStatusTransition,
                   $"Action '{typeof(TValue).Name}' has status '{currentStatus}', but expected status is '{expectedStatus}!",
                   ErrorType.Conflict
            );

        public static Error InvalidInputData<TValue>()
            => new(DomainErrorCodes.Action.InvalidInputData,
                   $"Invalid user input. Owner : {typeof(TValue).Name}!",
                   ErrorType.Validation
            );
    }
}
