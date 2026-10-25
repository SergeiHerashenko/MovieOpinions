using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses.Validation
{
    internal static partial class IpAddressValidator
    {
        /// <summary>
        /// Містить представлення IPv4-адреси та контекст доменної
        /// операції, що передаються правилам валідації.
        ///
        /// (Contains the IPv4-address representation and domain-operation
        /// context supplied to the validation rules.)
        /// </summary>
        private sealed class IpAddressValidationData : IHasOperationType
        {
            /// <inheritdoc/>
            public required OperationType OperationType { get; init; }

            /// <summary>
            /// Представлення IPv4-адреси, що перевіряється.
            ///
            /// (The IPv4-address representation being validated.)
            /// </summary>
            public required string Value { get; init; }
        }
    }
}
