using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.DomainPart
{
    internal static partial class EmailDomainValidator
    {
        /// <summary>
        /// Містить значення доменної частини email-адреси
        /// та контекст доменної операції, що передаються
        /// правилам валідації.
        ///
        /// (Contains the email domain value and domain-operation
        /// context supplied to the validation rules.)
        /// </summary>
        private sealed class EmailDomainValidationData : IHasOperationType
        {
            public required OperationType OperationType { get; init; }

            public required string Value { get; init; }
        }
    }
}
