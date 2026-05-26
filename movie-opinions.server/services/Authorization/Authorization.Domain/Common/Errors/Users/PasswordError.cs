namespace Authorization.Domain.Common.Errors.Users
{
    public static class PasswordError
    {
        public static Error Empty
            => new(ErrorCodes.PasswordError.Empty, "Password cannot be empty!", ErrorType.EmptyValue);
    }
}
