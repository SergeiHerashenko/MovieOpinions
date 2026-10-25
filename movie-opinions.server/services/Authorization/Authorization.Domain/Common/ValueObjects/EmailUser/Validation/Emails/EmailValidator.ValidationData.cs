using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Common.ValueObjects.EmailUser.Validation.Emails
{
    internal static partial class EmailValidator
    {
        /// <summary>
        /// Містить повне значення email-адреси та контекст
        /// доменної операції, що передаються правилам валідації.
        ///
        /// (Contains the complete email-address value and
        /// domain-operation context supplied to the validation rules.)
        /// </summary>
        private sealed class EmailValidationData : IHasOperationType
        {
            /// <inheritdoc/>
            public required OperationType OperationType { get; init; }

            /// <summary>
            /// Повне значення email-адреси, передане для перевірки.
            ///
            /// (The complete email-address value supplied for validation.)
            /// </summary>
            public required string Value { get; init; }
        }
    }
}
