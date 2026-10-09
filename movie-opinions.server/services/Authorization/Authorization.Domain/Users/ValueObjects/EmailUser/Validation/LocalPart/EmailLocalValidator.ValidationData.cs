using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.LocalPart
{
    internal static partial class EmailLocalValidator
    {
        /// <summary>
        /// Містить значення локальної частини email-адреси
        /// та контекст доменної операції, що передаються
        /// правилам валідації.
        ///
        /// (Contains the email local-part value and domain-operation
        /// context supplied to the validation rules.)
        /// </summary>
        private sealed class EmailLocalValidationData : IHasOperationType
        {
            public required OperationType OperationType { get; init; }

            public required string Value { get; init; }
        }
    }
}
