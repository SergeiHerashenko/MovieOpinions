namespace Authorization.Domain.Common.Errors.Users
{
    public static class EmailError
    {
        public static Error Empty
            => new(ErrorCodes.EmailError.Empty, "Email cannot be empty!", ErrorType.EmptyValue);

        public static Error InvalidFormat
            => new(ErrorCodes.EmailError.InvalidFormat, "Invalid email format!", ErrorType.Validation);

        public static Error TooLong
            => new(ErrorCodes.EmailError.TooLong, "Email is too long!", ErrorType.Validation);

        public static Error NotAllowed
            => new(ErrorCodes.EmailError.NotAllowed, "This email is not allowed!", ErrorType.BusinessRuleViolation);
    }
}
