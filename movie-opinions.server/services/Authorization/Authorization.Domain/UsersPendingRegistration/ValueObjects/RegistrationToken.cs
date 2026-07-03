using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using System.Security.Cryptography;

namespace Authorization.Domain.UsersPendingRegistration.ValueObjects
{
    public sealed class RegistrationToken : ValueObject
    {
        public string Value { get; }

        private const int TOKEN_BYTES = 64;

        private RegistrationToken(string value)
        {
            Value = value;
        }

        public static RegistrationToken CreateUnique()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TOKEN_BYTES));

            return new RegistrationToken(token);
        }

        public static RegistrationToken Restore(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
                throw DomainDataInconsistencyException.Empty<RegistrationToken>(nameof(value));

            return new RegistrationToken(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
