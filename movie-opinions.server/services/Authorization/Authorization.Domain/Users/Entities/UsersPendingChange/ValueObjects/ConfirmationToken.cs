using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using System.Security.Cryptography;

namespace Authorization.Domain.Users.Entities.UsersPendingChange.ValueObjects
{
    public sealed class ConfirmationToken : ValueObject
    {
        public string Value { get; }

        private const int TOKEN_BYTES = 64;
        
        private ConfirmationToken(string value)
        {
            Value = value;
        }

        #region Creation
        internal static ConfirmationToken Create()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TOKEN_BYTES));

            return new ConfirmationToken(token);
        }
        #endregion

        #region Restoration
        public static ConfirmationToken Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw DomainDataInconsistencyException.Empty<ConfirmationToken>(nameof(value));

            return new ConfirmationToken(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
