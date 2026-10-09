using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.PasswordUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Validation.ValidatedPlainPassword
{
    internal static partial class PlainPasswordValidator
    {
        /// <summary>
        /// Перевіряє, що довжина пароля не перевищує
        /// встановлене максимальне значення.
        ///
        /// Правило повинно виконуватися лише після успішного
        /// виконання <see cref="RequiredRule"/>.
        ///
        /// (Validates that the password length does not exceed
        /// the configured maximum.
        ///
        /// The rule must execute only after
        /// <see cref="RequiredRule"/> succeeds.)
        /// </summary>
        private sealed class TooLongPlainPasswordRule
            : IValidationRule<PlainPasswordValidationData, ErrorValidationFailure>
        {
            private const int MAX_LENGTH_PLAIN_PASSWORD = 64;

            public ValidationPriority Priority => ValidationPriority.Length;

            public ErrorValidationFailure? Validate(PlainPasswordValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw CreatePreconditionException(
                        nameof(TooLongPlainPasswordRule),
                        data.OperationType
                    );
                }

                if (data.Value.Length <= MAX_LENGTH_PLAIN_PASSWORD)
                    return null;

                return new ErrorValidationFailure()
                {
                    Error = PlainPasswordErrors.TooLongPlainPassword<PlainPassword>(
                        data.Value.Length,
                        MAX_LENGTH_PLAIN_PASSWORD
                    )
                };
            }
        }
    }
}
