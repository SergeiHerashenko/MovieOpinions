using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses.Rules
{
    public sealed class ValidIpAddressRule : IValidationRule<string, ValidationRestoreFailure>
    {
        private const int IPv4OctetCount = 4;

        public ValidationPriority Priority => ValidationPriority.Format;

        public ValidationRestoreFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (IsValidIPv4(value))
                return null;

            return new ValidationRestoreFailure()
            {
                Error = IpAddressErrors.InvalidFormatIpAddress<IpAddress>(),
                BuildException = () => DomainDataInconsistencyException.InvalidFieldFormat<IpAddress>(nameof(value), value)
            };
        }

        private static bool IsValidIPv4(string value)
        {
            var parts = value.Split('.');

            if (parts.Length != IPv4OctetCount)
                return false;

            foreach (var part in parts)
            {
                if (!byte.TryParse(part, out _))
                    return false;
            }

            return true;
        }
    }
}
