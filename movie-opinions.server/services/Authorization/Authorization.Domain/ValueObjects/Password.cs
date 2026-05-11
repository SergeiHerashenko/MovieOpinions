using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;

namespace Authorization.Domain.ValueObjects
{
    public sealed class Password
    {
        public string Value { get; }

        private Password(string value)
        {
            Value = value;
        }

        public static Password Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationDomainException(
                    ErrorCodes.PasswordError.Empty,
                    $"Password cannot be empty. Entity {nameof(Password)}!"
                );

            return new Password(value);
        }

        public override string ToString() => "********";
    }
}
