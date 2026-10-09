using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses.Validation
{
    internal static partial class IpAddressValidator
    {
        private sealed class IpAddressValidationData : IHasOperationType
        {
            public required OperationType OperationType { get; init; }

            public required string Value { get; init; }
        }
    }
}
