using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.Entities.UsersRestriction.Errors
{
    /// <summary>
    /// Містить фабрики очікуваних помилок валідації
    /// правила обмеження користувача.
    ///
    /// (Provides factories for expected user-restriction
    /// rule validation errors.)
    /// </summary>
    public static class RestrictionErrors
    {
        public static Error EmptyRestrictionName<TType>()
            => new(
                DomainErrorCodes.Restriction.EmptyNameRestriction,
                $"Restriction rule validation failed for type '{typeof(TType).Name}': " +
                "the rule name is empty or missing.",
                ErrorType.Validation
            );

        public static Error InvalidDuration<TType>(int actualDurationMinutes)
            => new(
                DomainErrorCodes.Restriction.InvalidNumberMinutes,
                $"Restriction rule validation failed for type '{typeof(TType).Name}': " +
                "duration must be greater than zero minutes, but the actual value is " +
                $"{actualDurationMinutes}.",
                ErrorType.Validation
            );

        public static Error AlreadyRevoked<TType>()
            => new(
                DomainErrorCodes.Restriction.AlreadyRevoked,
                $"Restriction status transition failed for type '{typeof(TType).Name}': " +
                "the restriction has already been revoked.",
                ErrorType.Conflict
            );

        public static Error AlreadyCompleted<TType>()
            => new(
                DomainErrorCodes.Restriction.AlreadyCompleted,
                $"Restriction status transition failed for type '{typeof(TType).Name}': " +
                "the restriction has already been completed.",
                ErrorType.Conflict
            );
    }
}
