namespace Authorization.Domain.Common.Errors.Users
{
    public static class UserError
    {
        public static Error Empty(string fieldName)
            => new(ErrorCodes.UserError.Empty,
                   $"Validation failed: value is null or empty. Entity {fieldName}",
                   ErrorType.EmptyValue);

        public static Error OperationIsNotAllowed(string message)
            => new(ErrorCodes.UserError.OperationIsNotAllowed,
                   $"Operation is not allowed. Details {message}",
                   ErrorType.InvalidOperation);

        public static Error UserIsBlocked
            => new(ErrorCodes.AccountStatusError.AccountBlocked,
                   "Operation not allowed for blocked user!",
                   ErrorType.Forbidden);

        public static Error UserIsDeleted
            => new(ErrorCodes.AccountStatusError.AccountDeleted,
                   "Operation not allowed for deleted user!",
                   ErrorType.Forbidden);

        public static Error NoChangesDetected(string message)
            => new(ErrorCodes.ChangeError.UpdateNotRequired,
                   $"{message}",
                   ErrorType.Conflict);
    }
}
