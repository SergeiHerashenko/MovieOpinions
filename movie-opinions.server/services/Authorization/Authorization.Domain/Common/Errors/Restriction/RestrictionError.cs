namespace Authorization.Domain.Common.Errors.Restriction
{
    public static class RestrictionError
    {
        public static Error Empty(string fieldName, string entityName)
            => new(ErrorCodes.RestrictionError.EmptyValue,
                   $"It is impossible to create an empty rule. Field {fieldName}. Entity {entityName}!",
                   ErrorType.EmptyValue
            );

        public static Error NotEnoughDays
            => new(ErrorCodes.RestrictionError.ShortDay,
                   "Restriction duration must be greater than zero",
                   ErrorType.BusinessRuleViolation
            );

        public static Error WrongTime
            => new(ErrorCodes.RestrictionError.InvalidTime,
                   "The time of lifting the restriction cannot be earlier than the restriction itself!",
                   ErrorType.BusinessRuleViolation
            );
    }
}
