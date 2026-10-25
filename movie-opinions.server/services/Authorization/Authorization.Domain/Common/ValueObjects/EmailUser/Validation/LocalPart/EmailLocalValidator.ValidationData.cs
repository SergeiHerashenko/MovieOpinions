using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Common.ValueObjects.EmailUser.Validation.LocalPart
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
            /// <inheritdoc/>
            public required OperationType OperationType { get; init; }

            /// <summary>
            /// Значення локальної частини email-адреси, передане для перевірки.
            ///
            /// (The email local-part value supplied for validation.)
            /// </summary>
            public required string Value { get; init; }
        }
    }
}
