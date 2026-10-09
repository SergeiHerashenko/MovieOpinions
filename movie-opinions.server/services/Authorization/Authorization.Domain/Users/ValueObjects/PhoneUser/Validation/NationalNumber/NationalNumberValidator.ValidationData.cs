using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Validation.NationalNumber
{
    internal static partial class NationalNumberValidator
    {
        /// <summary>
        /// Містить представлення національного номера та контекст
        /// доменної операції, що передаються правилам валідації.
        ///
        /// (Contains the national-number representation and domain-operation
        /// context supplied to the validation rules.)
        /// </summary>
        private sealed class NationalNumberValidationData : IHasOperationType
        {
            public required OperationType OperationType { get; init; }

            public required string Value { get; init; }
        }
    }
}
