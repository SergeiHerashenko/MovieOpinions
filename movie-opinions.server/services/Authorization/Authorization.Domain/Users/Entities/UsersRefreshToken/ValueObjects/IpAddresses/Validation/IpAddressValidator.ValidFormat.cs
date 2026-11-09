using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Common.Validation.Interfaces;
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

            /// <inheritdoc/>
            public ValidationPriority Priority => ValidationPriority.Format;

            /// <inheritdoc/>
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
                {
                    return null;
                }

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

            /// <summary>
            /// Визначає, чи відповідає значення канонічному формату IPv4:
            /// чотири десяткові октети від 0 до 255, розділені крапками,
            /// без початкових нулів.
            ///
            /// (Determines whether the value conforms to the canonical IPv4 format:
            /// four decimal octets from 0 to 255, separated by dots,
            /// without leading zeros.)
            /// </summary>
            /// <param name="value">
            /// Непорожнє представлення адреси для перевірки.
            ///
            /// (The non-empty address representation to validate.)
            /// </param>
            /// <returns>
            /// true, якщо формат коректний; інакше false.
            ///
            /// (true when the format is valid; otherwise, false.)
            /// </returns>
            private static bool IsValidIPv4(string value)
            {
                var octets = value.Split('.');

                if (octets.Length != IPV4_OCTET_COUNT)
                {
                    return false;
                }

                foreach (var octet in octets)
                {
                    if (octet.Length is < 1 or > MAX_IPV4_OCTET_LENGTH)
                    {
                        return false;
                    }

                    if (octet.Length > 1 && octet[0] == '0')
                    {
                        return false;
                    }

                    if (!octet.All(char.IsAsciiDigit))
                    {
                        return false;
                    }

                    if (!byte.TryParse(octet, out _))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
