using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;

namespace Authorization.Domain.Users.ValueObjects
{
    public sealed class Password : ValueObject
    {
        public string HashPassword { get; }

        private Password(string hashPassword)
        {
            HashPassword = hashPassword;
        }

        public static DomainResult<Password> Create(string hashPassword)
        {
            if (string.IsNullOrWhiteSpace(hashPassword))
                return DomainResult<Password>.Failure(PasswordError.Empty);

            return DomainResult<Password>.Success(new Password(hashPassword));
        }

        public static Password Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw DomainDataInconsistencyException.EmptyOnRestore<Password>(
                    nameof(value)
                );
            }

            return new Password(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return HashPassword;
        }
    }
}
