using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses.Errors;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses.Validation
{
    internal static partial class IpAddressValidator
    {
        /// <summary>
        /// Перевіряє наявність обов’язкового значення IPv4-адреси.
        ///
        /// (Validates the presence of the required IPv4-address value.)
        /// </summary>
        private sealed class RequiredRule : IValidationRule<IpAddressValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Presence;

            public ValidationFailure? Validate(IpAddressValidationData data)
            {
                if (!string.IsNullOrWhiteSpace(data.Value))
                    return null;

                return new ValidationFailure()
                {
                    Error = IpAddressErrors.Empty<IpAddress>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.Empty<IpAddress>(
                            nameof(IpAddress.Value),
                            operationType
                        )
                };
            }
        }
    }
}
