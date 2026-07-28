using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses.Rules
{
    public sealed class EmptyIpAddressRule : IValidationRule<string, ValidationRestoreFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Presence;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return null;

            return new ValidationRestoreFailure()
            {
                Error = IpAddressErrors.EmptyIpAddress<IpAddress>(),
                BuildException = () => DomainDataInconsistencyException.Empty<IpAddress>(nameof(value))
            };
        }
    }
}
