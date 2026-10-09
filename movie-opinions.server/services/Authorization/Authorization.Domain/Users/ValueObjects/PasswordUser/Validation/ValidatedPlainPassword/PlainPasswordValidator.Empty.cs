using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.PasswordUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Validation.ValidatedPlainPassword
{
    internal static partial class PlainPasswordValidator
    {
        /// <summary>
        /// Перевіряє, що пароль не є порожнім
        /// і не складається лише з пробільних символів.
        ///
        /// (Validates that the password is not empty
        /// and does not consist only of whitespace.)
        /// </summary>
        private sealed class RequiredRule : IValidationRule<PlainPasswordValidationData, ErrorValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Presence;

            public ErrorValidationFailure? Validate(PlainPasswordValidationData data)
            {
                if (!string.IsNullOrWhiteSpace(data.Value))
                    return null;

                return new ErrorValidationFailure()
                {
                    Error = PlainPasswordErrors.EmptyPlainPassword<PlainPassword>()
                };
            }
        }
    }
}
