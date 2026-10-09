using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.PasswordUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Validation.ValidatedPlainPassword
{
    internal static partial class PlainPasswordValidator
    {
        /// <summary>
        /// Перевіряє, що довжина пароля відповідає
        /// встановленому мінімальному значенню.
        ///
        /// Правило повинно виконуватися лише після успішного
        /// виконання <see cref="RequiredRule"/>.
        ///
        /// (Validates that the password length satisfies
        /// the configured minimum.
        ///
        /// The rule must execute only after
        /// <see cref="RequiredRule"/> succeeds.)
        /// </summary>
        private sealed class TooShortPlainPasswordRule
            : IValidationRule<PlainPasswordValidationData, ErrorValidationFailure>
        {
            private const int MIN_LENGTH_PLAIN_PASSWORD = 8;

            public ValidationPriority Priority => ValidationPriority.Length;

            public ErrorValidationFailure? Validate(PlainPasswordValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw CreatePreconditionException(
                        nameof(TooShortPlainPasswordRule),
                        data.OperationType
                    );
                }

                if (data.Value.Length >= MIN_LENGTH_PLAIN_PASSWORD)
                    return null;

                return new ErrorValidationFailure()
                {
                    Error = PlainPasswordErrors.TooShortPlainPassword<PlainPassword>(
                        data.Value.Length,
                        MIN_LENGTH_PLAIN_PASSWORD
                    )
                };
            }
        }
    }
}
