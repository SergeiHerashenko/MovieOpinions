using Authorization.Domain.Common.Exceptions.DomainException;
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
        /// Видаляє зовнішні пробільні символи та створює IPv4-адресу
        /// після успішної перевірки канонічного формату.
        ///
        /// (Removes surrounding whitespace and creates an IPv4 address
        /// after successful validation of its canonical format.)
        /// </summary>
        /// <param name="rawIpAddress">
        /// Зовнішнє представлення IPv4-адреси. Значення null
        /// розглядається як очікувана помилка валідації.
        ///
        /// (The external IPv4-address representation. A null value
        /// is treated as an expected validation error.)
        /// </param>
        /// <returns>
        /// Успішний результат із канонічною IPv4-адресою
        /// або перша виявлена помилка валідації.
        ///
        /// (A successful result containing the canonical IPv4 address,
        /// or the first detected validation error.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal validation-rule precondition
        /// or the required rule execution order is violated.)
        /// </exception>
        public static Result<IpAddress> Create(string? rawIpAddress)
        {
            var normalizedIpAddress = rawIpAddress?.Trim() ?? string.Empty;

            var failure = IpAddressValidator.ValidateForError(
                OperationType.Create,
                normalizedIpAddress
            );

            if (failure is not null)
            {
                return Result<IpAddress>.Failure(failure.Value);
            }

            return Result<IpAddress>.Success(new IpAddress(normalizedIpAddress));
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює IPv4-адресу зі збереженого канонічного значення
        /// без його нормалізації.
        ///
        /// (Restores an IPv4 address from its persisted canonical value
        /// without normalizing it.)
        /// </summary>
        /// <param name="value">
        /// Збережене канонічне значення IPv4-адреси.
        ///
        /// (The persisted canonical IPv4-address value.)
        /// </param>
        /// <returns>
        /// Відновлена IPv4-адреса.
        ///
        /// (The restored IPv4 address.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережене значення відсутнє
        /// або не відповідає канонічному формату IPv4.
        ///
        /// (Thrown when the persisted value is missing
        /// or does not conform to the canonical IPv4 format.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal validation-rule precondition
        /// or the required rule execution order is violated.)
        /// </exception>
        public static IpAddress Restore(string value)
        {
            var failure = IpAddressValidator.ValidateForException(
                OperationType.Restore,
                value
            );

            if (failure is not null)
            {
                throw failure.Value;
            }

            return new IpAddress(value);
        }
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
