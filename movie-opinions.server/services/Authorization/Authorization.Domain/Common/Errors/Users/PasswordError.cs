namespace Authorization.Domain.Common.Errors.Users
{
    public static class PasswordError
    {
        public static Error Empty
            => new(ErrorCodes.PasswordError.EmptyPassword, "Password cannot be empty!", ErrorType.EmptyValue);
    }
}
