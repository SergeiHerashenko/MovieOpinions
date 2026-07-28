using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using System.Security.Cryptography;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects
{
    public sealed class RefreshToken : ValueObject
    {
        public string Value { get; }

        private const int REFRESH_TOKEN_BYTES = 64;

        private RefreshToken(string value)
        {
            Value = value;
        }

        #region Creation
        public static RefreshToken Create()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(REFRESH_TOKEN_BYTES));

            return new RefreshToken(token);
        }
        #endregion

        #region Restoration
        public static RefreshToken Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw DomainDataInconsistencyException.Empty<RefreshToken>(nameof(value));

            return new RefreshToken(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
