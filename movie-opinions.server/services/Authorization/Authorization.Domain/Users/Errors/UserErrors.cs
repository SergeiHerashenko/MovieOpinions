using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.Errors
{
    /// <summary>
    /// Містить фабрики очікуваних доменних помилок, пов’язаних
    /// зі станом користувача, доступом, обмеженнями,
    /// незавершеними діями та токенами.
    ///
    /// (Provides factories for expected domain errors related
    /// to user state, access, restrictions, pending actions, and tokens.)
    /// </summary>
    public static class UserErrors
    {
        public static Error UserDeleted<TType>()
            => new(
                DomainErrorCodes.AccessUser.UserDeleted,
                $"Access is denied because the user is deleted. Owner: '{typeof(TType).Name}'.",
                ErrorType.Forbidden
            );

        public static Error UserBlocked<TType>()
            => new(
                DomainErrorCodes.AccessUser.UserBlocked,
                $"Access is denied because the user is blocked. Owner: '{typeof(TType).Name}'.",
                ErrorType.Forbidden
            );

        public static Error EmptyRestrictionList<TType>()
            => new(
                DomainErrorCodes.RestrictionUser.EmptyRestrictionList,
                $"Restriction list is empty or missing. Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error DuplicateRestrictionIdentifiers<TType>()
            => new(
                DomainErrorCodes.RestrictionUser.DuplicateRestrictionIdentifiers,
                $"Restriction list contains duplicate identifiers. Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error NotFoundRestrictionSession<TType>(RestrictionType restrictionType)
            => new(
                DomainErrorCodes.RestrictionUser.NotFoundSession,
                $"Restriction session was not found for restriction type '{restrictionType}'. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.NotFound
            );

        public static Error NotFoundRestriction<TType>(IEnumerable<UserRestrictionId> missingIds)
            => new(
                DomainErrorCodes.RestrictionUser.NotFoundRestriction,
                $"Restrictions were not found. " +
                $"Missing identifiers: [{string.Join(", ", missingIds.Select(x => x.Value))}]. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.NotFound
            );

        public static Error NoUpdateNeeded<TType>()
            => new(
                DomainErrorCodes.PendingAction.NoUpdateNeeded,
                $"No update is needed for the requested pending action. Owner: '{typeof(TType).Name}'.",
                ErrorType.Conflict
            );

        public static Error ActionAlreadyExists<TType>()
            => new(
                DomainErrorCodes.PendingAction.ActionAlreadyExists,
                $"A pending action already exists. Owner: '{typeof(TType).Name}'.",
                ErrorType.Conflict
            );

        public static Error EmptyAction<TType>()
            => new(
                DomainErrorCodes.PendingAction.EmptyAction,
                $"Pending action is empty or missing. Owner: '{typeof(TType).Name}'.",
                ErrorType.NotFound
            );

        public static Error NotFoundAction<TType>()
            => new(
                DomainErrorCodes.PendingAction.NotFoundAction,
                $"Pending action was not found. Owner: '{typeof(TType).Name}'.",
                ErrorType.NotFound
            );

        public static Error InvalidActionType<TType>()
            => new(
                DomainErrorCodes.PendingAction.InvalidActionType,
                $"The current pending action type does not match the requested operation. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Conflict
            );

        public static Error UserNotDeleted<TType>()
            => new(
                DomainErrorCodes.UserDeletion.UserNotDeleted,
                $"User is not in the deleted state. Owner: '{typeof(TType).Name}'.",
                ErrorType.Conflict
            );

        public static Error NotFoundToken<TType>()
            => new(
                DomainErrorCodes.TokenStatus.NotFoundToken,
                $"The requested token was not found. Owner: '{typeof(TType).Name}'.",
                ErrorType.NotFound
            );

        public static Error UserAlreadyDeleted<TType>()
            => new(
                DomainErrorCodes.UserDeletion.UserAlreadyDeleted,
                $"User account is already deleted. Owner: '{typeof(TType).Name}'.",
                ErrorType.Conflict
            );
    }
}
