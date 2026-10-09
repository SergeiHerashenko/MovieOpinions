using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.Errors
{
    public static class UserErrors
    {
        public static Error UserDeleted<TType>()
            => new(
                DomainErrorCodes.AccessUser.UserDeleted,
                $"",
                ErrorType.Forbidden
            );

        public static Error UserBlocked<TType>()
            => new(
                DomainErrorCodes.AccessUser.UserBlocked,
                $"",
                ErrorType.Forbidden
            );

        public static Error EmptyRestrictionList<TType>()
            => new(
                DomainErrorCodes.RestrictionUser.EmptyRestrictionList,
                $"",
                ErrorType.Validation
            );

        public static Error DuplicateRestrictionIdentifiers<TType>()
            => new(
                DomainErrorCodes.RestrictionUser.DuplicateRestrictionIdentifiers,
                $"",
                ErrorType.Conflict
            );

        public static Error NotFoundRestrictionSession<TType>(RestrictionType restrictionType)
            => new(
                DomainErrorCodes.RestrictionUser.NotFoundSession,
                $"",
                ErrorType.NotFound
            );

        public static Error NotFoundRestriction<TType>(IEnumerable<UserRestrictionId> missingIds)
            => new(
                DomainErrorCodes.RestrictionUser.NotFoundSession,
                $"{string.Join(", ", missingIds.Select(x => x.Value))}",
                ErrorType.NotFound
            );
    }
}
