using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;

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
        private static readonly ValidationOrchestrator<IpAddressValidationData, ValidationFailure> _validation = new(
            [
                new RequiredRule(),
                new ValidFormatRule()
            ]
        );

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
