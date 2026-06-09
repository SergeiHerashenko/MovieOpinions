using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;
using System.Security.Cryptography;

namespace Authorization.Domain.UsersRefreshToken.ValueObjects
{
    public sealed class RefreshToken : ValueObject
    {
        public string Value { get; }

        private const int REFRESH_TOKEN_BYTES = 64;

        private RefreshToken(string value) => Value = value;

        public static RefreshToken CreateUnique()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(REFRESH_TOKEN_BYTES));

            return new RefreshToken(token);
        }

        public static RefreshToken Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"Missing required field {nameof(value)} during {nameof(RefreshToken)} entity reconstruction!",
                    new Dictionary<string, object>
                    {
                        ["entity"] = nameof(RefreshToken),
                        ["field"] = nameof(value),
                        ["operation"] = "restore",
                    }
                );

            return new RefreshToken(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
