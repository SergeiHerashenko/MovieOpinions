using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser
{
    public sealed class Password : ValueObject
    {
        private readonly PasswordHash _hash;

        private Password(PasswordHash hash)
        {
            _hash = hash;
        }

        public string Value => _hash.Value;

        #region Creation
        public static Result<Password> Create(PasswordHash hash)
        {
            if (hash is null)
                return Result<Password>.Failure(PasswordErrors.EmptyHashPassword<Password>());

            return Result<Password>.Success(new Password(hash));
        }
        #endregion

        #region Restoration
        public static Password Restore(PasswordHash hash)
        {
            DomainGuard.AgainstNull<Password>((hash, nameof(hash)));

            return new Password(hash);
        }

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return _hash.Value;
        }
        #endregion
    }
}
