using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.CountryCode
{
    internal static partial class CountryCodeValidator
    {
        /// <summary>
        /// Містить значення телефонного коду та контекст доменної
        /// операції, що передаються правилам валідації.
        ///
        /// (Contains the telephone country-code value and domain-operation
        /// context supplied to the validation rules.)
        /// </summary>
        private sealed class CountryCodeValidationData : IHasOperationType
        {
            /// <inheritdoc/>
            public required OperationType OperationType { get; init; }

            /// <summary>
            /// Значення телефонного коду країни, передане для перевірки.
            ///
            /// (The country calling code supplied for validation.)
            /// </summary>
            public required string Value { get; init; }
        }
    }
}
