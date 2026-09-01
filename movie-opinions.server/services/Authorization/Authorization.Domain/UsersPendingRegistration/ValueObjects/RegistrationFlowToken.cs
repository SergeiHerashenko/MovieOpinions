using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.UsersPendingRegistration.Errors;
using System.Security.Cryptography;

namespace Authorization.Domain.UsersPendingRegistration.ValueObjects
{
    /// <summary>
    /// Криптографічно випадковий непрозорий ідентифікатор
    /// реєстраційного потоку.
    ///
    /// (Cryptographically random opaque identifier
    /// of a registration flow.)
    /// </summary>
    public sealed class RegistrationFlowToken : ValueObject
    {
        public string Value { get; }

        private const int TokenSizeInBytes = 64;
        private const int EncodedTokenLength = ((TokenSizeInBytes + 2) / 3) * 4;

        private RegistrationFlowToken(string value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Генерує новий криптографічно випадковий токен
        /// реєстраційного потоку.
        ///
        /// (Generates a new cryptographically random registration-flow token.)
        /// </summary>
        internal static RegistrationFlowToken Create()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenSizeInBytes));

            return new RegistrationFlowToken(token);
        }
        #endregion

        #region Parse
        /// <summary>
        /// Перевіряє та перетворює закодоване значення
        /// на RegistrationFlowToken.
        ///
        /// (Validates and parses an encoded value into a RegistrationFlowToken.)
        /// </summary>
        /// <param name="rawRegistrationFlowToken">Закодоване значення токена.</param>
        /// <returns>Успішний результат із токеном або помилки його валідації.</returns>
        public static Result<RegistrationFlowToken> Parse(string? rawRegistrationFlowToken)
        {
            if (string.IsNullOrWhiteSpace(rawRegistrationFlowToken))
                return Result<RegistrationFlowToken>.Failure(RegistrationFlowTokenErrors.Empty<RegistrationFlowToken>());

            if (rawRegistrationFlowToken.Length != EncodedTokenLength)
                return Result<RegistrationFlowToken>.Failure(RegistrationFlowTokenErrors.InvalidLength<RegistrationFlowToken>(
                    rawRegistrationFlowToken.Length,
                    EncodedTokenLength)
                );

            Span<byte> tokenBytes = stackalloc byte[TokenSizeInBytes];

            if (!Convert.TryFromBase64String(
                rawRegistrationFlowToken,
                tokenBytes,
                out var bytesWritten) ||
                bytesWritten != TokenSizeInBytes)
            {
                return Result<RegistrationFlowToken>.Failure(RegistrationFlowTokenErrors.InvalidFormat<RegistrationFlowToken>());
            }

            var registrationFlowToken = new RegistrationFlowToken(rawRegistrationFlowToken);

            return Result<RegistrationFlowToken>.Success(registrationFlowToken);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
