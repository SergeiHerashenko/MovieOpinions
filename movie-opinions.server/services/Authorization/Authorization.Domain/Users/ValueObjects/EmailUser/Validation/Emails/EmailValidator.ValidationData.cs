using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.Emails
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
            public required OperationType OperationType { get; init; }

            public required string Value { get; init; }
        }
    }
}
