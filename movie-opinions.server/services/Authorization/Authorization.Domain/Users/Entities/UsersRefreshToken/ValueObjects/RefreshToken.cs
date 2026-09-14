using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using System.Security.Cryptography;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects
{
    /// <summary>
    /// Криптографічно випадковий непрозорий refresh token.
    /// Його сире значення є конфіденційним і не повинно
    /// потрапляти в логи чи діагностичні повідомлення.
    ///
    /// (Cryptographically random opaque refresh token.
    /// Its raw value is confidential and must not be written
    /// to logs or diagnostic messages.)
    /// </summary>
    public sealed class RefreshToken : ValueObject
    {
        private const int REFRESH_TOKEN_SIZE_IN_BYTES = 64;

        private const int ENCODED_TOKEN_LENGTH = ((REFRESH_TOKEN_SIZE_IN_BYTES + 2) / 3) * 4;

        /// <summary>
        /// Сире закодоване значення refresh token.
        ///
        /// (Raw encoded refresh-token value.)
        /// </summary>
        public string Value { get; }

        private RefreshToken(string value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Генерує новий криптографічно випадковий refresh token.
        ///
        /// (Generates a new cryptographically random refresh token.)
        /// </summary>
        internal static RefreshToken Create()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(REFRESH_TOKEN_SIZE_IN_BYTES));

            return new RefreshToken(token);
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює refresh token зі збереженого значення
        /// та перевіряє його структурну коректність.
        ///
        /// (Restores a refresh token from its persisted value
        /// and validates its structural correctness.)
        /// </summary>
        /// <param name="value">Збережене закодоване значення токена.</param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо значення порожнє, має неправильну довжину
        /// або не є валідним Base64-токеном очікуваного розміру.
        /// </exception>
        public static RefreshToken Restore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw DomainDataInconsistencyException.Empty<RefreshToken>(nameof(value));

            if (value.Length != ENCODED_TOKEN_LENGTH)
            {
                throw DomainDataInconsistencyException.ValueOutOfRange<RefreshToken>(
                    nameof(value),
                    value.Length,
                    OperationType.Restore,
                    context: new Dictionary<string, object>
                    {
                        ["ActualLength"] = value.Length,
                        ["ExpectedLength"] = ENCODED_TOKEN_LENGTH
                    }
                );
            }

            Span<byte> tokenBytes = stackalloc byte[REFRESH_TOKEN_SIZE_IN_BYTES];
             
            if (!Convert.TryFromBase64String(
                    value,
                    tokenBytes,
                    out var bytesWritten)
                || bytesWritten != REFRESH_TOKEN_SIZE_IN_BYTES)
            {
                throw DomainDataInconsistencyException.InvalidFieldFormat<RefreshToken>(
                    nameof(value),
                    value
                );
            }

            return new RefreshToken(value);
        }
        #endregion

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
