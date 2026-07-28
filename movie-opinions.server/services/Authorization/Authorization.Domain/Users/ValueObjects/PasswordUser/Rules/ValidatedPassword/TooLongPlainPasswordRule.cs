using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Rules.ValidatedPassword
{
    public sealed class TooLongPlainPasswordRule : IValidationRule<string, ValidationFailure>
    {
        private const int MAX_LENGTH_PLAIN_PASSWORD = 64;

        public ValidationPriority Priority => ValidationPriority.Length;

        public ValidationFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.Length < MAX_LENGTH_PLAIN_PASSWORD)
                return null;

            return new ValidationFailure()
            {
                Error = PasswordErrors.TooLongPassword<PlainPassword>()
            };
        }
    }
}
