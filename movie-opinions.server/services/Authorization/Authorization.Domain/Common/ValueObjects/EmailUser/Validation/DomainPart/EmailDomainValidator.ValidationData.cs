using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Common.ValueObjects.EmailUser.Validation.DomainPart
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
            /// <inheritdoc/>
            public required OperationType OperationType { get; init; }

            /// <summary>
            /// Значення доменної частини email-адреси, передане для перевірки.
            ///
            /// (The email domain-part value supplied for validation.)
            /// </summary>
            public required string Value { get; init; }
        }
    }
}
