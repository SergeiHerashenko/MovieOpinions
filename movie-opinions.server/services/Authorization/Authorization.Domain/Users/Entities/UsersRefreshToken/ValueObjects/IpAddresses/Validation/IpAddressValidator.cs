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
        private static readonly ValidationOrchestrator<string, ValidationFailure> _validation = new(
            [
                new RequiredRule(),
                new ValidFormatRule()
            ]
        );

        internal static ValidationRulesFailure<Error>? ValidateForError(string value)
        {
            var failure = _validation.Validate(value);

            if (failure is null)
                return null;

            return new ValidationRulesFailure<Error>(failure.Error);
        }

        internal static ValidationRulesFailure<Exception>? ValidateForException(
            string value,
            OperationType operationType)
        {
            var failure = _validation.Validate(value);

            if (failure is null)
                return null;

            var exception = failure.BuildException(operationType);

            return new ValidationRulesFailure<Exception>(exception);
        }
    }
}
