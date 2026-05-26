using Authorization.Domain.Common.Errors.Users;
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

        public static Result<Password> Create(string hashPassword)
        {
            if (string.IsNullOrWhiteSpace(hashPassword))
                return Result<Password>.Failure(PasswordError.Empty);

            return Result<Password>.Success(new Password(hashPassword));
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return HashPassword;
        }
    }
}
