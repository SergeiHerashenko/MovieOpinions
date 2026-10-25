using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.Phones
{
    internal static partial class PhoneValidator
    {
        /// <summary>
        /// Містить складові телефонного номера та контекст
        /// доменної операції, що передаються правилам валідації.
        ///
        /// (Contains telephone-number components and domain-operation
        /// context supplied to the validation rules.)
        /// </summary>
        private sealed class PhoneValidationData : IHasOperationType
        {
            /// <inheritdoc/>
            public required OperationType OperationType { get; init; }

            /// <summary>
            /// Міжнародний код країни, переданий для перевірки.
            ///
            /// (The international country code supplied for validation.)
            /// </summary>
            public required string CountryCode { get; init; }

            /// <summary>
            /// Національна частина телефонного номера, передана для перевірки.
            ///
            /// (The national part of the telephone number supplied for validation.)
            /// </summary>
            public required string NationalNumber { get; init; }
        }
    }
}
