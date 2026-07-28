using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Common.Errors.Users
{
    public static class ChangeErrors
    {
        public static Error EmptyChangeUser<TValue>()
            => new(DomainErrorCodes.Change.EmptyChange,
                   $"Chamge is missing or empty. Owner: {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );

        public static Error Expired<TValue>()
            => new(DomainErrorCodes.General.Expired,
                   $"Change already expired. Owner: {typeof(TValue).Name}!",
                   ErrorType.InvalidOperation
            );

        public static Error InvalidConfirmationToken<TValue>()
            => new(DomainErrorCodes.Change.InvalidConfirmationToken,
                   $"Confirmation token invslid. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error ChangeAlreadyExists<TValue>()
            => new(DomainErrorCodes.Change.ChangeAlreadyExists,
                   $"The change already exists. Owner: {typeof(TValue).Name}!",
                   ErrorType.Conflict
            );

        public static Error InvalidChangeType<TValue>()
            => new(DomainErrorCodes.Change.InvalidChangeType,
                   $"Invalod change type. Owner: {typeof(TValue).Name}!",
                   ErrorType.Conflict
            );
    }
}
