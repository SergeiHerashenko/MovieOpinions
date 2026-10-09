using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.PasswordUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Validation.ValidatedPlainPassword
{
    internal static partial class PlainPasswordValidator
    {
        /// <summary>
        /// Перевіряє наявність щонайменше однієї цифри.
        ///
        /// Правило повинно виконуватися лише після успішного
        /// виконання <see cref="RequiredRule"/>.
        ///
        /// (Validates the presence of at least one digit.
        ///
        /// The rule must execute only after
        /// <see cref="RequiredRule"/> succeeds.)
        /// </summary>
        private sealed class DigitRequiredRule : IValidationRule<PlainPasswordValidationData, ErrorValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Format;

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
                    return null;

                return new ErrorValidationFailure()
                {
                    Error = PlainPasswordErrors.MissingDigitPlainPassword<PlainPassword>()
                };
            }
        }
    }
}
