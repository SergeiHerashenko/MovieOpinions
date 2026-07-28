using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Rules.ValidatedPassword
{
    public sealed class TooShortPlainPassword : IValidationRule<string, ValidationFailure>
    {
        private const int MIN_LENGTH_PLAIN_PASSWORD = 8;

        public ValidationPriority Priority => ValidationPriority.Length;

        public ValidationFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.Length > MIN_LENGTH_PLAIN_PASSWORD)
                return null;

            return new ValidationFailure()
            {
                Error = PasswordErrors.TooShortPassword<PlainPassword>()
            };
        }
    }
}
