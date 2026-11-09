using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Common.Validation.Interfaces;
using Authorization.Domain.Common.ValueObjects.PasswordUser.Errors;

namespace Authorization.Domain.Common.ValueObjects.PasswordUser.Validation.ValidatedPlainPassword
{
    internal static partial class PlainPasswordValidator
    {
        /// <summary>
        /// Перевіряє наявність щонайменше однієї великої літери.
        ///
        /// Правило повинно виконуватися лише після успішного
        /// виконання <see cref="RequiredRule"/>.
        ///
        /// (Validates the presence of at least one uppercase letter.
        ///
        /// The rule must execute only after
        /// <see cref="RequiredRule"/> succeeds.)
        /// </summary>
        private sealed class UppercaseLetterRequiredRule
            : IValidationRule<PlainPasswordValidationData, ErrorValidationFailure>
        {
            /// <inheritdoc/>
            public ValidationPriority Priority => ValidationPriority.Format;

            /// <inheritdoc/>
            public ErrorValidationFailure? Validate(PlainPasswordValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw CreatePreconditionException(
                         nameof(UppercaseLetterRequiredRule),
                         data.OperationType
                    );
                }

                if (data.Value.Any(char.IsUpper))
                {
                    return null;
                }

                return new ErrorValidationFailure()
                {
                    Error = PlainPasswordErrors.MissingUppercaseLetterPlainPassword<PlainPassword>()
                };
            }
        }
    }
}
