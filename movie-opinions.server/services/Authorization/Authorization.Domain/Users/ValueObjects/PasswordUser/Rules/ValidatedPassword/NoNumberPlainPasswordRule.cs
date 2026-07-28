using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PasswordUser.Rules.ValidatedPassword
{
    public class NoNumberPlainPasswordRule : IValidationRule<string, ValidationFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Format;

        public ValidationFailure? Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.Any(char.IsDigit))
                return null;

            return new ValidationFailure()
            {
                Error = PasswordErrors.MissingNumberPlainPassword<PlainPassword>()
            };
        }
    }
}
