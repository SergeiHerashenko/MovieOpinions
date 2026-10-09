using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses.Errors;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses.Validation
{
    internal static partial class IpAddressValidator
    {
        /// <summary>
        /// Перевіряє, що значення відповідає канонічному десятковому
        /// формату IPv4: чотири октети в діапазоні від 0 до 255
        /// без початкових нулів.
        ///
        /// Правило повинно виконуватися лише після успішної перевірки
        /// наявності значення правилом <see cref="RequiredRule"/>.
        ///
        /// (Validates that the value uses the canonical dotted-decimal
        /// IPv4 format: four octets in the range from 0 to 255
        /// without leading zeros.
        ///
        /// The rule must only execute after the value presence has been
        /// successfully validated by <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class ValidFormatRule : IValidationRule<IpAddressValidationData, ValidationFailure>
        {
            private const int IPV4_OCTET_COUNT = 4;

            private const int MAX_IPV4_OCTET_LENGTH = 3;

            public ValidationPriority Priority => ValidationPriority.Format;

            public ValidationFailure? Validate(IpAddressValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<IpAddress>(
                        nameof(ValidFormatRule),
                        nameof(RequiredRule),
                        data.OperationType
                    );
                }

                if (IsValidIPv4(data.Value))
                    return null;

                return new ValidationFailure()
                {
                    Error = IpAddressErrors.InvalidFormat<IpAddress>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.InvalidFieldFormat<IpAddress>(
                            nameof(IpAddress.Value),
                            data.Value,
                            operationType
                        )
                };
            }

            private static bool IsValidIPv4(string value)
            {
                var octets = value.Split('.');

                if (octets.Length != IPV4_OCTET_COUNT)
                    return false;

                foreach (var octet in octets)
                {
                    if (octet.Length is < 1 or > MAX_IPV4_OCTET_LENGTH)
                        return false;

                    if (octet.Length > 1 && octet[0] == '0')
                        return false;

                    if (!octet.All(char.IsAsciiDigit))
                        return false;

                    if (!byte.TryParse(octet, out _))
                        return false;
                }

                return true;
            }
        }
    }
}
