using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;

namespace Authorization.Domain.Users.ValueObjects
{
    public sealed class Password : ValueObject
    {
        public string Value { get; }

        private Password(string hashPassword)
        {
            Value = hashPassword;
        }

        public static Result<Password> Create(string hashPassword)
        {
            if (string.IsNullOrWhiteSpace(hashPassword))
                return Result<Password>.Failure(UserErrors.PasswordError.EmptyPassword<Password>());

            return Result<Password>.Success(new Password(hashPassword));
        }

        public static Password Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw DomainDataInconsistencyException.Empty<Password>(nameof(value));
            }

            return new Password(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
