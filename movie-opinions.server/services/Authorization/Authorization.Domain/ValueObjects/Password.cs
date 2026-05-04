using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;

namespace Authorization.Domain.ValueObjects
{
    public sealed class Password
    {
        public string Hash { get; }

        private Password(string hash)
        {
            Hash = hash;
        }

        public static Password Create(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ValidationDomainException(ErrorCodes.PasswordError.Empty, "Пароль не може бути пустим!");

            return new Password(hash);
        }

        public override string ToString() => "********";
    }
}
