using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Common.Errors.Users
{
    public static class RestrictionErrors
    {
        public static Error EmptyRestrictionList<TValue>()
            => new(DomainErrorCodes.Restriction.EmptyRestrictionList,
                   $"The list of restrictions is empty or missing. Owner: {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );

        public static Error EmptyRestriction<TValue>()
           => new(DomainErrorCodes.Restriction.EmptyRestriction,
                  $"The restriction is empty or missing. Owner: {typeof(TValue).Name}!",
                  ErrorType.EmptyValue
           );

        public static Error EmptyNameRestriction<TValue>()
            => new(DomainErrorCodes.Restriction.EmptyRestrictionName,
                   $"Restriction name, empty or missing. Owner: {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );

        public static Error EmptyRestrictionRule<TValue>()
            => new(DomainErrorCodes.Restriction.EmptyRestrictionRule,
                   $"The rules restriction is empty or missing. Owner: {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );

        public static Error InvalidRestrictionType<TValue>(string nameType)
            => new(DomainErrorCodes.Restriction.InvalidRestrictionType,
                   $"The passed type constraint {nameType} is not compatible with the specified type. Owner: {typeof(TValue).Name}!",
                   ErrorType.UnsupportedType
            );

        public static Error NotFoundRestriction<TValue>()
            => new(DomainErrorCodes.Restriction.NotFoundRestriction,
                   $"The restraction was not found for this user. Owner: {typeof(TValue).Name}!",
                   ErrorType.NotFound
            );

        public static Error InvalidNumberMinutes<TValue>()
            => new(DomainErrorCodes.Restriction.InvalidNumberMinutes,
                   $"Invalid number of minutes, or negative value. Owner: {typeof(TValue).Name}!",
                   ErrorType.InvalidFormat
            );

        public static Error WrongTime<TValue>()
            => new(DomainErrorCodes.Restriction.WrongTime,
                   $"The time of lifting the restriction cannot be earlier than the restriction itself. Owner: {typeof(TValue).Name}!",
                   ErrorType.InvalidFormat
            );

        public static Error NotFoundSession<TValue>(string nameRestriction)
            => new(DomainErrorCodes.RestrictionSession.NotFoundSession,
                   $"No session was found for this constraint '{nameRestriction}'. Owner: {typeof(TValue).Name}!",
                   ErrorType.NotFound
            );

        public static Error NotFoundSessionType<TValue>(string nameType)
            => new(DomainErrorCodes.RestrictionSession.NotFoundSessionType,
                   $"No session found for this type '{nameType}'. Owner: {typeof(TValue).Name}!",
                   ErrorType.NotFound
            );

        public static Error UserIsBlocked<TValue>()
            => new(DomainErrorCodes.Access.UserIsBlocked,
                   $"User blocked. Owner: {typeof(TValue).Name}!",
                   ErrorType.Forbidden
            );
    }
}
