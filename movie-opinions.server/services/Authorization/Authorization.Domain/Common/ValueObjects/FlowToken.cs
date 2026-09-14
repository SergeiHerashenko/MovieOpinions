using Authorization.Domain.Common.Models;
using System.Security.Cryptography;

namespace Authorization.Domain.Common.ValueObjects
{
    /// <summary>
    /// Базовий клас криптографічно випадкових непрозорих
    /// токенів доменних потоків.
    /// Централізує правила генерації, довжини, формату та рівності токенів.
    ///
    /// (Base class for cryptographically random opaque domain flow tokens.
    /// Centralizes token generation, length, format, and equality rules.)
    /// </summary>
    public abstract class FlowToken : ValueObject
    {
        private const int TOKEN_SIZE_IN_BYTES = 64;
        private const int ENCODED_TOKEN_LENGTH = (TOKEN_SIZE_IN_BYTES + 2) / 3 * 4;

        /// <summary>
        /// Закодоване значення токена.
        ///
        /// (Encoded token value.)
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Очікувана довжина Base64-представлення токена.
        ///
        /// (Expected length of the Base64-encoded token.)
        /// </summary>
        protected static int ExpectedEncodedLength => ENCODED_TOKEN_LENGTH;

        protected FlowToken(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Генерує нове криптографічно випадкове значення токена.
        ///
        /// (Generates a new cryptographically random token value.)
        /// </summary>
        protected static string GenerateValue()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(TOKEN_SIZE_IN_BYTES));
        }

        /// <summary>
        /// Перевіряє довжину закодованого значення токена.
        ///
        /// (Validates the length of the encoded token value.)
        /// </summary>
        protected static bool HasExpectedLength(string value)
        {
            return value.Length == ENCODED_TOKEN_LENGTH;
        }

        /// <summary>
        /// Перевіряє, що значення є коректним Base64-представленням
        /// токена очікуваного розміру.
        ///
        /// (Validates that the value is a valid Base64 representation
        /// of a token with the expected size.)
        /// </summary>
        protected static bool HasValidFormat(string value)
        {
            Span<byte> tokenBytes = stackalloc byte[TOKEN_SIZE_IN_BYTES];

            return Convert.TryFromBase64String(
                value,
                tokenBytes,
                out var bytesWritten
            ) && bytesWritten == TOKEN_SIZE_IN_BYTES;
        }

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
