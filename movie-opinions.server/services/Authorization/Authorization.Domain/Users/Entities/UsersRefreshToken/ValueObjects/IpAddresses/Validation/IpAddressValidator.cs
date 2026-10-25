using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Errors;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses.Validation
{
    /// <summary>
    /// Координує повний набір правил валідації IPv4-адреси
    /// та формує відповідне представлення порушення.
    ///
    /// (Coordinates the complete IPv4-address validation rule set
    /// and produces the appropriate failure representation.)
    /// </summary>
    internal static partial class IpAddressValidator
    {
        /// <summary>
        /// Містить упорядкований набір правил перевірки
        /// наявності значення та канонічного формату IPv4-адреси.
        ///
        /// (Contains the ordered rule set for validating
        /// value presence and the canonical IPv4-address format.)
        /// </summary>
        private static readonly ValidationOrchestrator<IpAddressValidationData, ValidationFailure> _validation = new(
            [
                new RequiredRule(),
                new ValidFormatRule()
            ]
        );

        /// <summary>
        /// Перевіряє значення IPv4-адреси та повертає
        /// першу виявлену очікувану доменну помилку.
        ///
        /// (Validates an IPv4-address value and returns
        /// the first detected expected domain error.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="value">
        /// Представлення IPv4-адреси для перевірки.
        ///
        /// (The IPv4-address representation to validate.)
        /// </param>
        /// <returns>
        /// Обгортка з першою виявленою помилкою або null,
        /// якщо всі правила виконані.
        ///
        /// (A wrapper containing the first detected error,
        /// or null when all rules pass.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил.
        ///
        /// (Thrown when an internal rule precondition
        /// or the required rule execution order is violated.)
        /// </exception>
        internal static ValidationRulesFailure<Error>? ValidateForError(
            OperationType operationType,
            string value)
        {
            var data = BuildData(
                operationType,
                value
            );

            return DomainValidationExecutor.ValidateForError(
                _validation,
                data
            );
        }

        /// <summary>
        /// Перевіряє значення IPv4-адреси та для першого порушення
        /// створює об’єкт доменного винятку, не кидаючи його.
        ///
        /// Винятки порушення внутрішніх передумов правил
        /// передаються безпосередньо виклику.
        ///
        /// (Validates an IPv4-address value and creates a domain exception
        /// object for the first validation failure without throwing it.
        ///
        /// Internal rule-precondition exceptions propagate directly
        /// to the caller.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="value">
        /// Представлення IPv4-адреси для перевірки.
        ///
        /// (The IPv4-address representation to validate.)
        /// </param>
        /// <returns>
        /// Обгортка зі створеним винятком або null,
        /// якщо всі правила виконані.
        ///
        /// (A wrapper containing the constructed exception,
        /// or null when all rules pass.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил.
        ///
        /// (Thrown when an internal rule precondition
        /// or the required rule execution order is violated.)
        /// </exception>
        internal static ValidationRulesFailure<Exception>? ValidateForException(
            OperationType operationType,
            string value)
        {
            var data = BuildData(
                operationType,
                value
            );

            return DomainValidationExecutor.ValidateForException(
                _validation,
                data
            );
        }

        private static IpAddressValidationData BuildData(
            OperationType operationType,
            string value)
        {
            return new IpAddressValidationData()
            {
                OperationType = operationType,
                Value = value
            };
        }
    }
}
