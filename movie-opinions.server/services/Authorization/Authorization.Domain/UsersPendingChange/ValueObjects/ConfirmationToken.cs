using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using System.Security.Cryptography;

namespace Authorization.Domain.UsersPendingChange.ValueObjects
{
    public sealed class ConfirmationToken : ValueObject
    {
        public string Value { get; }

        private const int TOKEN_BYTES = 64;

        private ConfirmationToken(string value) => Value = value;

        public static ConfirmationToken CreateUnique()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TOKEN_BYTES));

            return new ConfirmationToken(token);
        }

        public static ConfirmationToken Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw DomainDataInconsistencyException.Empty<ConfirmationToken>(nameof(value));

            return new ConfirmationToken(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
