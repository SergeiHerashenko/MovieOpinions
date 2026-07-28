using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using System.Security.Cryptography;

namespace Authorization.Domain.UsersPendingRegistration.ValueObjects
{
    public sealed class RegistrationFlowToken : ValueObject
    {
        public string Value { get; }

        private const int TOKEN_BYTES = 64;

        private RegistrationFlowToken(string value)
        {
            Value = value;
        }

        #region Creation
        public static RegistrationFlowToken Create()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TOKEN_BYTES));

            return new RegistrationFlowToken(token);
        }
        #endregion

        #region Restoration
        public static RegistrationFlowToken Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw DomainDataInconsistencyException.Empty<RegistrationFlowToken>(nameof(value));

            return new RegistrationFlowToken(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
