namespace Authorization.Domain.Common.Errors.Restriction
{
    public static class RestrictionError
    {
        public static Error Empty<TValue>(string fieldName)
            => new(DomainErrorCodes.RestrictionRuleErrorCode.EmptyValue,
                   $"It is impossible to create an empty rule. Field {fieldName}. Owner {typeof(TValue).Name}!",
                   ErrorType.EmptyValue
            );

        public static Error NotEnoughDays<TValue>()
            => new(DomainErrorCodes.RestrictionRuleErrorCode.ShortDay,
                   $"Restriction duration must be greater than zero. Owner {typeof(TValue).Name}!",
                   ErrorType.PolicyViolation
            );

        public static Error WrongTime<TValue>()
            => new(DomainErrorCodes.RestrictionRuleErrorCode.InvalidTime,
                   $"The time of lifting the restriction cannot be earlier than the restriction itself. Owner {typeof(TValue).Name}!",
                   ErrorType.PolicyViolation
            );
    }
}
