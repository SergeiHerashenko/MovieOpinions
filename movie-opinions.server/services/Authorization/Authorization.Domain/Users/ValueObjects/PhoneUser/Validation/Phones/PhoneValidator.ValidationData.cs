using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Validation.Phones
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
            public required OperationType OperationType { get; init; }

            public required string CountryCode { get; init; }

            public required string NationalNumber { get; init; }
        }
    }
}
