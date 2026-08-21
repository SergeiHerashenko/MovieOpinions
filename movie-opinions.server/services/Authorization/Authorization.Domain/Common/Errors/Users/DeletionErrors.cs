using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Common.Errors.Users
{
    public static class DeletionErrors
    {
        public static Error Expired<TValue>(string fieldName)
            => new(DomainErrorCodes.General.Expired,
                   $"The expiration date for '{fieldName}' has expired. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error NotDeleteUser<TValue>()
            => new(DomainErrorCodes.Deletion.NotDeleteUser,
                   $"The user has no active deletion. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error UserIsDeleted<TValue>()
            => new(DomainErrorCodes.Access.UserIsDeleted,
                   $"User deleted. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );

        public static Error TooLongReason<TValue>()
            => new(DomainErrorCodes.Deletion.TooLongReason,
                   $"This description of the reason for deletion is too long. Owner: {typeof(TValue).Name}!",
                   ErrorType.Validation
            );

        public static Error NotFoundAction<TValue>()
            => new(DomainErrorCodes.Deletion.NotFoundAction,
                   $"The user has no active actions to change. Owner: {typeof(TValue).Name}!",
                   ErrorType.Conflict
            );
    }
}
