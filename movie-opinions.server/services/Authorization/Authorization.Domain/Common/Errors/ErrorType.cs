namespace Authorization.Domain.Common.Errors
{
    public enum ErrorType
    {
        EmptyValue,

        BusinessRuleViolation,

        InvariantViolation,

        InvalidOperation,

        Validation,

        Forbidden,

        Conflict
    }
}
