using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses.Validation;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses
{
    /// <summary>
    /// Представляє нормалізовану IPv4-адресу.
    ///
    /// (Represents a normalized IPv4 address.)
    /// </summary>
    public sealed class IpAddress : ValueObject
    {
        /// <summary>
        /// Канонічне десяткове представлення IPv4-адреси.
        ///
        /// (Canonical dotted-decimal representation of the IPv4 address.)
        /// </summary>
        public string Value { get; }

        private IpAddress(string value)
        {
            Value = value;
        }

        #region Creation
        /// <summary>
        /// Нормалізує та перевіряє зовнішнє значення IP-адреси.
        ///
        /// (Normalizes and validates an external IP-address value.)
        /// </summary>
        public static Result<IpAddress> Create(string? rawIpAddress)
        {
            var normalizedIpAddress = rawIpAddress?.Trim() ?? string.Empty;

            var failure = IpAddressValidator.ValidateForError(normalizedIpAddress);

            if (failure is not null)
                return Result<IpAddress>.Failure(failure.Value);

            return Result<IpAddress>.Success(new IpAddress(normalizedIpAddress)); 
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює IP-адресу зі збереженого значення.
        ///
        /// (Restores an IP address from its persisted value.)
        /// </summary>
        /// <param name="value">Збережене значення IPv4-адреси.</param>
        /// <returns>Відновлена IPv4-адреса.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення не є валідною IPv4-адресою.
        /// </exception>
        public static IpAddress Restore(string value)
        {
            var failure = IpAddressValidator.ValidateForException(
                value,
                OperationType.Restore
            );

            if (failure is not null)
                throw failure.Value;

            return new IpAddress(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
