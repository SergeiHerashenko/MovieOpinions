using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Common.Validation.Interfaces;
using Authorization.Domain.Common.ValueObjects.PasswordUser.Errors;

namespace Authorization.Domain.Common.ValueObjects.PasswordUser.Validation.ValidatedPlainPassword
{
    internal static partial class PlainPasswordValidator
    {
        /// <summary>
        /// Перевіряє наявність щонайменше однієї ASCII-цифри від '0' до '9'.
        ///
        /// Правило повинно виконуватися лише після успішного
        /// виконання <see cref="RequiredRule"/>.
        ///
        /// (Validates the presence of at least one ASCII digit from '0' to '9'.
        ///
        /// The rule must execute only after
        /// <see cref="RequiredRule"/> succeeds.)
        /// </summary>
        private sealed class DigitRequiredRule : IValidationRule<PlainPasswordValidationData, ErrorValidationFailure>
        {
            /// <inheritdoc/>
            public ValidationPriority Priority => ValidationPriority.Format;

            /// <inheritdoc/>
            public ErrorValidationFailure? Validate(PlainPasswordValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw CreatePreconditionException(
                        nameof(DigitRequiredRule),
                        data.OperationType
                    );
                }

                if (data.Value.Any(char.IsAsciiDigit))
                {
                    return null;
                }

                return new ErrorValidationFailure()
                {
                    Error = PlainPasswordErrors.MissingDigitPlainPassword<PlainPassword>()
                };
            }
        }
    }
}
